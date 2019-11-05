module AST
    // TODO: shouldn't binary operators contain their left/right hand sides
    //       as child nodes?
    type Expression =
        | Variable of string
        | ExprNum of float
        | Brackets of Expression
        | Add of Expression * Expression
        | Subtract of Expression * Expression
        | Divide of Expression * Expression
        | Multiply of Expression * Expression

    type Account = Account of string list

    type Commodity = Commodity of string

    type Amount =
        | Amount of Commodity * float
        | AmountExpression of Commodity * Expression

    type Transaction =
        | Trx of Account * Amount option

    type Posting = Posting of Transaction list

    type Column = Column of string

    type BooleanOperator =
        | Equals
        | NotEquals
        | GreaterThan
        | GreaterThanOrEqualTo
        | LessThan
        | LessThanOrEqualTo
        | Substring
        | Matches

    type FilterAtom =
        | Regex of string
        | String of string
        | Number of float

    type Filter =
        | Equals                of Column * FilterAtom
        | NotEquals             of Column * FilterAtom
        | GreaterThan           of Column * FilterAtom
        | GreaterThanOrEqualTo  of Column * FilterAtom
        | LessThan              of Column * FilterAtom
        | LessThanOrEqualTo     of Column * FilterAtom
        | Substring             of Column * FilterAtom
        | Matches               of Column * FilterAtom

    type Payee = Payee of string

    type Description = Description of Payee * Filter list * Posting

