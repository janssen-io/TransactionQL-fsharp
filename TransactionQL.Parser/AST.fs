module AST
    type Word = Word of string
    type String = String of string
    type Regex = Regex of string
    type Number = Number of float
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
    type Transaction = Transaction of Account * Commodity * Expression
    //type Posting = Posting of Transaction list
    //type Column = Column of string
    //type Filter = Filter of Column * BooleanOperator * FilterAtom
    //type Payee = Payee of string
    //type Description = Description of Payee * Filter list * Posting


