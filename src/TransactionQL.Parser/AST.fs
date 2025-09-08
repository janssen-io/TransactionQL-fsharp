namespace TransactionQL.Parser

module AST =
    type Expression =
        | Variable of string
        | ExprNum of float
        | Add of Expression * Expression
        | Subtract of Expression * Expression
        | Divide of Expression * Expression
        | Multiply of Expression * Expression

    type Account = 
        | AccountLiteral of string list
        | AccountVariable of string

    type Commodity = Commodity of string

    type Amount =
        | Amount of Commodity * float
        | AmountExpression of Commodity * Expression

    type Transaction =
        { Account: Account
          Amount: Amount option
          Tags: string array }

    type Posting = Posting of string option * Transaction list

    type Column = Column of string

    type Metadata = Metadata of string * string

    type FilterAtom =
        | RegExp of string
        | String of string
        | Number of float

    type FilterOperator =
        | EqualTo
        | NotEqualTo
        | GreaterThan
        | GreaterThanOrEqualTo
        | LessThan
        | LessThanOrEqualTo
        | Contains
        | Matches

    type Filter =
        | Filter of Column * FilterOperator * FilterAtom
        | OrGroup of Filter list

    type Payee = 
        | Word of string
        | ColumnToken of Column
        | Interpolation of Payee list

    type Query = Query of Payee * Filter list * Posting
