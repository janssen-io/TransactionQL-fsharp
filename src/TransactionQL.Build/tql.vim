syn keyword tqlLanguageKeywords posting Posting
syn match tqlHeader '^# .*'
syn match tqlAccount /\w\+\(:\w\+\)\+/

syn match tqlNumber '\d\+'
syn match tqlNumber '\d\+\.\d*'

syn match tqlColumn '^\(or \)\?\zs[A-Z][a-z]*\ze'
syn match tqlVariable '[a-zA-Z]\+' contained

syn region tqlString start='"' end='"'
syn region tqlRegex start='/' end='/'
syn region tqlExpr start='(' end=')' contained contains=tqlVariable
syn region tqlPosting start='{' end='}' contains=tqlExpr,tqlAccount

let b:current_syntax = "tql"
hi def link tqlHeader Underlined
hi def link tqlString Constant
hi def link tqlRegex Constant
hi def link tqlNumber Constant
hi def link tqlAccount Identifier
hi def link tqlVariable Identifier
hi def link tqlColumn Identifier
