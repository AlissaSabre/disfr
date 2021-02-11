%namespace disfr.po
%scannertype PoScanner
%visibility internal
%tokentype Token

%option unicode, minimize, parser, persistbuffer, noembedbuffers, squeeze

%%

\n							MsgIdToken = Token.MSGID;
[ \t\r\v\f]					;

[#]\n						return (int)Token.TRCOM;

[#][|]|[#][~][|]			MsgIdToken = Token.P_MSGID;
[#][~]						MsgIdToken = Token.O_MSGID;

[#][.][^\n]*\n				yylval = TrimText(yytext); return (int)Token.EXCOM;
[#][:][^\n]*\n				yylval = TrimText(yytext); return (int)Token.RFCOM;
[#][,][^\n]*\n				yylval = TrimText(yytext); return (int)Token.FLAGS;
[#][^|~.:,\n][^\n]*\n		yylval = TrimText(yytext); return (int)Token.TRCOM;

[A-Za-z_]+					return (int)Keyword(yytext);

\[							return (int)Token.OB;
\]							return (int)Token.CB;

[0-9]+						yylval = yytext; return (int)Token.NUMBER;

["]([^"\\]|[\\][^\n])*["]	yylval = DecodeEscaped(yytext); return (int)Token.STRING;

.							yyerror("Invalid character U+{1:X4} '{0}'", (int)yytext[0], yytext[0]);
