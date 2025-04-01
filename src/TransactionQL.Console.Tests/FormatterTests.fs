module FormatterTests

open Xunit
open TransactionQL.Application
open TransactionQL.Application.Format
open TransactionQL.Parser.QLInterpreter
open System

let line (account, amount, tag) =
    { Account = account
      Amount = amount
      Tags = tag }
    : Line

[<Fact>]
let ``Header: starts with formatted date`` () =
    let header = Header(new DateTime(2019, 4, 27), "some title")

    let result =
        Formatter.sprintHeader
            { Date = "yyyy-MM-dd"
              Precision = 2
              Comment = "# " }
            header

    Assert.StartsWith("2019-04-27", result)

[<Fact>]
let ``Header: ends with the payee`` () =
    let header = Header(new DateTime(2019, 4, 27), "some title")

    let result =
        Formatter.sprintHeader
            { Date = "yyyy-MM-dd"
              Precision = 2
              Comment = "# " }
            header

    Assert.EndsWith("some title", result)

[<Fact>]
let ``Line: is indented`` () =
    let line = line ([ "A"; "B" ], None, [||])

    let result =
        Formatter.sprintLine
            { Date = "yyyy-MM-dd"
              Precision = 2
              Comment = "# " }
            0
            line

    Assert.StartsWith("  ", result)

[<Fact>]
let ``Line: concatenates accounts with colon`` () =
    let line = line ([ "A"; "B" ], None, [||])

    let result =
        Formatter.sprintLine
            { Date = "yyyy-MM-dd"
              Precision = 2
              Comment = "# " }
            0
            line

    Assert.Contains("A:B", result)

[<Fact>]
let ``Line: separates accounts and commodity with at least two spaces`` () =
    let line = line ([ "A"; "B" ], Some("$", 25.00), [||])

    let result =
        Formatter.sprintLine
            { Date = "yyyy-MM-dd"
              Precision = 2
              Comment = "# " }
            0
            line

    let indexAccount = result.IndexOf("B")
    let indexCommodity = result.IndexOf("$")

    Assert.True(
        indexCommodity - indexAccount > 2,
        "There are less than two spaces between the account and the commodity"
    )

[<Fact>]
let ``Line: prints the float with the given precision`` () =
    let format: Format =
        { Date = "yyyy-MM-dd"
          Precision = 3
          Comment = "# " }

    let amount = 25.12345678
    let line = line ([ "A"; "B" ], Some("€", amount), [||])
    let result = Formatter.sprintLine format 0 line
    Assert.EndsWith("25.123", result)

[<Fact>]
let ``Line: Adds tags (if any) after two spaces`` () =
    let format: Format =
        { Date = "yyyy-MM-dd"
          Precision = 2
          Comment = "; " }

    let amount = 25.12345678
    let line = line ([ "A"; "B" ], Some("€", amount), [|"My: Tag"|])
    let result = Formatter.sprintLine format 0 line
    Assert.EndsWith("  ; My: Tag", result)

[<Fact>]
let ``Posting: prints header and lines on separate lines`` () =
    let posting =
        { Header = Header(new DateTime(2019, 1, 1), "Payee")
          Lines =
            [ line ([ "A"; "B" ], Some("€", 10.00), [||])
              line ([ "C"; "D" ], None, [||]) ]
          Comments = [] }

    let result =
        Formatter.sprintPosting
            { Date = "yyyy-MM-dd"
              Precision = 2
              Comment = "# " }
            (fun _ -> [])
            id
            posting

    let lines = result.Split(Environment.NewLine)
    Assert.Equal(3, lines.Length)

[<Fact>]
let ``Posting: aligns amounts to the right`` () =
    let posting =
        { Header = Header(new DateTime(2019, 1, 1), "Payee")
          Lines =
            [ line ([ "Assets"; "Checking" ], Some("€", 10.), [||])
              line ([ "Expenses"; "Vacation" ], Some("$", -1000.), [||]) ]
          Comments = [] }

    let result =
        Formatter.sprintPosting
            { Date = "yyyy-MM-dd"
              Precision = 2
              Comment = "# " }
            (fun _ -> [])
            id
            posting

    let lines = result.Split(Environment.NewLine)
    Assert.Equal(lines.[1].Length, lines.[2].Length)
    Assert.EndsWith("   10.00", lines.[1])
    Assert.EndsWith("-1000.00", lines.[2])

[<Fact>]
let ``Missing posting: adds comment before each line`` () =
    let posting =
        { Header = Header(new DateTime(2019, 1, 1), "Payee")
          Lines =
            [ line ([ "A"; "B" ], Some("€", 10.00), [||])
              line ([ "C"; "D" ], None, [||]) ]
          Comments = [] }

    let format: Format =
        { Date = "yyyy-MM-dd"
          Precision = 2
          Comment = "# " }

    let result = Formatter.sprintMissingPosting format (fun _ -> []) posting
    let lines = result.Split(Environment.NewLine)
    Array.map (fun l -> Assert.StartsWith(format.Comment, l)) lines

[<Fact>]
let ``Comments: comments are added between the header and transactions`` () =
    let posting =
        { Header = Header(new DateTime(2019, 1, 1), "Payee")
          Lines =
            [ line ([ "A"; "B" ], Some("€", 10.00), [||])
              line ([ "C"; "D" ], None, [||]) ]
          Comments = [ "Two lines"; "Of comments" ] }

    let format: Format =
        { Date = "yyyy-MM-dd"
          Precision = 2
          Comment = "# " }

    let result = Formatter.sprintPosting format (fun _ -> []) id posting
    let lines = result.Split(Environment.NewLine)

    let expected = [| "# Two lines"; "# Of comments" |]
    expected = ((Array.tail >> Array.take 2) lines)
