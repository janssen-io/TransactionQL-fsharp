module AST
    type Word = Word of string

    // TODO: shouldn't binary operators contain their left/right hand sides
    //       as child nodes?
    type ArithmeticOperator = Add | Subtract | Divide | Multiply
    type BooleanOperator = 
        | Equals 
        | NotEquals 
        | GreaterThan 
        | GreaterThanOrEqualTo 
        | LessThan 
        | LessThanOrEqualTo 
        | Substring
        | Matches

    type Expression =
        | ExprWord of string
        | ExprNum of float
        | Expression of Expression * ArithmeticOperator * Expression

    type Account = Account of string list
    type Commodity = Commodity of string
    type Amount = 
        | Amount of Commodity * float
        | AmountExpression of Commodity * Expression

    type Transaction =
        | Trx of Account * Amount option

    type Posting = Posting of Transaction list
    type Column = Column of string

    type FilterExpression =
        | Regex of string
        | String of string
        | Number of float

    type Filter = Filter of Column * BooleanOperator * FilterExpression
    type Payee = Payee of string
    type Description = Description of Payee * Filter list * Posting


