module AST
    type Expression =
        | Variable of string
        | ExprNum of float
        | Add of Expression * Expression
        | Subtract of Expression * Expression
        | Divide of Expression * Expression
        | Multiply of Expression * Expression

    type Account = Account of string list

    type Commodity = Commodity of string

    type Amount =
        | Amount of Commodity * float
        | AmountExpression of Commodity * Expression

    type Transaction = | Trx of Account * Amount option

    type Posting = Posting of Transaction list

    type Column = Column of string

    type FilterAtom =
        | RegExp of string
        | String of string
        | Number of float

    type FilterOperator = 
        | EqualTo       | NotEqualTo
        | GreaterThan   | GreaterThanOrEqualTo
        | LessThan      | LessThanOrEqualTo
        | Contains      | Matches

    type Filter =
        | Filter of Column * FilterOperator * FilterAtom
        | OrGroup of Filter list

    type Payee = Payee of string

    type Query = Query of Payee * Filter list * Posting

    type Program = Program of Query list
