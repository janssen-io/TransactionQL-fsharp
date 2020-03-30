module FormatterTests

open Xunit
open TransactionQL.Console
open TransactionQL.Console.Format
open TransactionQL.Parser.QLInterpreter
open TransactionQL.Parser.AST
open System

[<Fact>]
let ``Header: starts with formatted date`` () =
    let header = Header (new DateTime(2019, 4, 27), "some title")
    let result = Formatter.sprintHeader { Date = "yyyy-MM-dd"; Precision = 2; Comment = "# " } header
    Assert.StartsWith("2019-04-27", result)

[<Fact>]
let ``Header: ends with the payee`` () =
    let header = Header (new DateTime(2019, 4, 27), "some title")
    let result = Formatter.sprintHeader { Date = "yyyy-MM-dd"; Precision = 2; Comment = "# " } header
    Assert.EndsWith("some title", result)

[<Fact>]
let ``Line: is indented`` () =
    let line = Line (Account ["A"; "B"], None)
    let result = Formatter.sprintLine { Date = "yyyy-MM-dd"; Precision = 2; Comment = "# " } 0 line 
    Assert.StartsWith("  ", result)

[<Fact>]
let ``Line: concatenates accounts with colon`` () =
    let line = Line (Account ["A"; "B"], None)
    let result = Formatter.sprintLine { Date = "yyyy-MM-dd"; Precision = 2; Comment = "# " } 0 line 
    Assert.Contains("A:B", result)

[<Fact>]
let ``Line: separates accounts and commodity with at least two spaces`` () =
    let line = Line (Account ["A"; "B"], Some (Commodity "$", 25.00))
    let result = Formatter.sprintLine { Date = "yyyy-MM-dd"; Precision = 2; Comment = "# " } 0 line 
    let indexAccount = result.IndexOf("B")
    let indexCommodity = result.IndexOf("$")
    Assert.True(indexCommodity - indexAccount > 2, "There are less than two spaces between the account and the commodity")

[<Fact>]
let ``Line: prints the float with the given precision`` () =
    let format : Format = { Date = "yyyy-MM-dd"; Precision = 3; Comment = "# " }
    let amount = 25.12345678
    let line = Line (Account ["A"; "B"], Some (Commodity "€", amount))
    let result = Formatter.sprintLine format 0 line 
    Assert.EndsWith("25.123", result)

[<Fact>]
let ``Posting: prints header and lines on separate lines`` () =
    let posting = 
        { Header = Header (new DateTime(2019, 1, 1), "Payee")
          Lines = [
              Line (Account ["A"; "B"], Some (Commodity "€", 10.00))
              Line (Account ["C"; "D"], None)
          ]
          Comments = [] }
    let result = Formatter.sprintPosting { Date = "yyyy-MM-dd"; Precision = 2; Comment = "# " } (fun _ -> []) id posting
    let lines = result.Split(Environment.NewLine)
    Assert.Equal(3, lines.Length)

[<Fact>]
let ``Posting: aligns amounts to the right`` () =
    let posting =
        { Header = Header (new DateTime(2019, 1, 1), "Payee")
          Lines = [
              Line (Account ["Assets"; "Checking"], Some (Commodity "€", 10.))
              Line (Account ["Expenses"; "Vacation"], Some (Commodity "$", -1000.))
          ]
          Comments = [] }
    let result = Formatter.sprintPosting { Date = "yyyy-MM-dd"; Precision = 2; Comment = "# " } (fun _ -> []) id posting
    let lines = result.Split(Environment.NewLine)
    Assert.Equal(lines.[1].Length, lines.[2].Length)
    Assert.EndsWith("   10.00", lines.[1])
    Assert.EndsWith("-1000.00", lines.[2])

[<Fact>]
let ``Missing posting: adds comment before each line`` () =
    let posting = 
        { Header = Header (new DateTime(2019, 1, 1), "Payee")
          Lines = [
              Line (Account ["A"; "B"], Some (Commodity "€", 10.00))
              Line (Account ["C"; "D"], None)
          ]
          Comments = [] }
    let format : Format = { Date = "yyyy-MM-dd"; Precision = 2; Comment = "# " }
    let result = Formatter.sprintMissingPosting format (fun _ -> []) posting
    let lines = result.Split(Environment.NewLine)
    Array.map (fun l -> Assert.StartsWith(format.Comment, l)) lines

[<Fact>]
let ``Comments: comments are added between the header and transactions`` () =
    let posting = 
        { Header = Header (new DateTime(2019, 1, 1), "Payee")
          Lines = [
              Line (Account ["A"; "B"], Some (Commodity "€", 10.00))
              Line (Account ["C"; "D"], None)
          ]
          Comments = 
            [ "Two lines"
              "Of comments" ] }
    let format : Format = { Date = "yyyy-MM-dd"; Precision = 2; Comment = "# " }
    let result = Formatter.sprintPosting format (fun _ -> []) id posting
    let lines = result.Split(Environment.NewLine)

    let expected = 
        [|"# Two lines"
          "# Of comments" |]
    expected = ((Array.tail >> Array.take 2) lines)

