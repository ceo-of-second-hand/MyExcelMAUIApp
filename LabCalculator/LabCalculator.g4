grammar LabCalculator;

compileUnit : expression EOF;

expression :
    LPAREN expression RPAREN #ParenthesizedExpr
    | operatorToken=(INCREMENT|DECREMENT) expression #IncDecExpr
    | expression EXPONENT expression #ExponentialExpr
    | expression operatorToken=(MULTIPLY | DIVIDE) expression #MultiplicativeExpr
    | expression operatorToken=(MOD | DIV) expression #HalfMultiplicativeExpr
    | expression operatorToken=(ADD | SUBTRACT) expression #AdditiveExpr
    | NUMBER #NumberExpr
    | IDENTIFIER #IdentifierExpr
    ;

NUMBER : INT ('.' INT)?; 
IDENTIFIER : [A-Z][0-9]+; 

INT : ('0'..'9')+;

MULTIPLY : '*';
DIVIDE : '/';
MOD : 'mod';
DIV : 'div';
SUBTRACT : '-';
EXPONENT : '^';
ADD : '+';
INCREMENT : 'inc';
DECREMENT : 'dec';
LPAREN : '(';
RPAREN : ')';

WS : [ \t\r\n] -> channel(HIDDEN);
