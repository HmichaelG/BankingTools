﻿namespace OfxNet.IntegrationTests;

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using Microsoft;
using Microsoft.VisualStudio.TestTools.UnitTesting;

[TestClass]
public class OfxDocumentTests
{
    public static IEnumerable<object[]> SampleOfxFiles
    {
        get
        {
            yield return new object[] { "SampleBankStatement-1.ofx", 1, 3 };
            yield return new object[] { "SampleBankStatement-2.ofx", 1, 2 };
            yield return new object[] { "SampleCreditCardStatement.ofx", 1, 1 };
            yield return new object[] { "SampleMultiStatement.ofx", 2, 3 };
            yield return new object[] { "SampleSignOnResponse.ofx", 0, 0 };
            yield return new object[] { "Sample-itau.ofx", 1, 3 };
            yield return new object[] { "Sample-santander.ofx", 1, 3 };
            yield return new object[] { "Sample-Banco do Brasil.ofx", 1, 3 };
        }
    }

    [TestInitialize]
    public void Setup()
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
    }

    [DataTestMethod]
    [DynamicData(nameof(SampleOfxFiles), DynamicDataSourceType.Property)]
    [SuppressMessage("Style", "IDE0060:Remove unused parameter", Justification = "Required for testing.")]
    public void OfxDocumentLoad_Succeeds(string path, int statementCount, int txCount)
    {
        var actual = OfxDocument.Load(path);
        Assert.IsNotNull(actual);
    }

    [DataTestMethod]
    [DynamicData(nameof(SampleOfxFiles), DynamicDataSourceType.Property)]
    public void OfxDocumentLoad_GetStatements_ReturnsCorrectNumberOfStatementsAndTransactions(string path, int statementCount, int txCount)
    {
        var actual = OfxDocument.Load(path);
        Assert.IsNotNull(actual);

        IEnumerable<OfxStatement> allStatements = actual.GetStatements();
        Assert.AreEqual(statementCount, allStatements.Count());

        IEnumerable<OfxStatementTransaction> allTransactions = allStatements.SelectMany(s => s.TransactionList!.Transactions);
        Assert.AreEqual(txCount, allTransactions.Count());
    }

    [TestMethod]
    public void CanParseItau()
    {
        IEnumerable<OfxStatement> actual = OfxDocument.Load(@"Sample-itau.ofx")
            .GetStatements();

        OfxStatement statement = actual.First();
        Assert.IsInstanceOfType(statement, typeof(OfxBankStatement));
        OfxBankStatement? bankStatement = statement as OfxBankStatement;
        Assert.IsNotNull(bankStatement);
        Assert.IsNotNull(bankStatement.Account);

        Assert.AreEqual("9999 99999-9", bankStatement.Account.AccountNumber);
        Assert.AreEqual("0341", bankStatement.Account.BankId);

        Assert.IsNotNull(statement.TransactionList);
        Assert.AreEqual(3, statement.TransactionList.Transactions.Count);
        CollectionAssert.AreEqual(
            statement.TransactionList.Transactions.Select(x => x.Memo).ToArray(),
            new string[] { "RSHOP", "REND PAGO APLIC AUT MAIS", "SISDEB" });
    }

    [TestMethod]
    public void CanParseBancoDoBrasil()
    {
        IEnumerable<OfxStatement> actual = OfxDocument.Load("Sample-Banco do Brasil.ofx")
            .GetStatements();

        OfxStatement statement = actual.First();
        Assert.IsInstanceOfType(statement, typeof(OfxBankStatement));
        var bankStatement = statement as OfxBankStatement;
        Assert.IsNotNull(bankStatement);
        Assert.IsNotNull(bankStatement.Account);

        Assert.AreEqual(bankStatement.Account.AccountNumber, "99999-9");
        Assert.AreEqual(bankStatement.Account.BranchId, "9999-9");
        Assert.AreEqual(bankStatement.Account.BankId, "1");

        Assert.IsNotNull(statement.TransactionList);
        Assert.AreEqual(3, statement.TransactionList.Transactions.Count);

        CollectionAssert.AreEqual(
            statement.TransactionList.Transactions.Select(x => x.Memo).ToArray(),
            new string[] { "Transferência Agendada", "Compra com Cartão", "Saque" });
    }
}
