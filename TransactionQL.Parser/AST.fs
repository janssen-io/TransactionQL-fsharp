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
        | Substring of String 
        | Matches of Regex

    //type Expression =
    //    | ExprWord of Word
    //    | ExprNum of Number
    //    | Expression of Expression * ArithmeticOperator * Expression

    //type FilterAtom =
    //    | Regex
    //    | String
    //    | Number

    //type Account = Account of Word list
    ////type Transaction = Transaction of Account * Commodity * Expression
    //type Posting = Posting of Transaction list
    //type Column = Column of string
    //type Filter = Filter of Column * BooleanOperator * FilterAtom
    //type Payee = Payee of string
    //type Description = Description of Payee * Filter list * Posting


