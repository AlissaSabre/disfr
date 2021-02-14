%namespace disfr.po
%partial
%parsertype PoParser
%visibility internal
%tokentype Token
%YYSTYPE string

%start file

%token	MSGID, P_MSGID, O_MSGID
%token	MSGCTXT, MSGID_PLURAL, MSGSTR
%token	DOMAIN
%token	TRCOM, EXCOM, RFCOM, FLAGS
%token	STRING NUMBER
%token	OB, CB

%%

file		: /* empty */
			| file domain
			| file def
			;

domain		: comments DOMAIN text				{ Sink.SetDomain($3); }
			;

def			: comments msg						{ Sink.FinishMessage(false); }
			| comments prev msg					{ Sink.FinishMessage(false); }
			| comments o_msg					{ Sink.FinishMessage(true); }
			| comments prev o_msg				{ Sink.FinishMessage(true); }
			;

comments	: /* empty */						{ Sink.Reset(); }
			| comments TRCOM					{ Sink.AddTranslatorComment($2); }
			| comments EXCOM					{ Sink.AddExtractedComment($2); }
			| comments RFCOM					{ Sink.AddReferences($2); }
			| comments FLAGS					{ Sink.AddFlags($2); }
			;

prev		: opt_msgctxt p_msgid				{ Sink.MakePrevious(); }
			| opt_msgctxt p_msgid msgid_plural	{ Sink.MakePrevious(); }
			;

msg			: opt_msgctxt msgid msgstr
			| opt_msgctxt msgid msgid_plural msgstr_plural_list
			;

o_msg		: opt_msgctxt o_msgid msgstr
			| opt_msgctxt o_msgid msgid_plural msgstr_plural_list
			;

opt_msgctxt	: /* empty */
			| MSGCTXT text						{ Sink.SetMsgCtxt($2); }
			;

msgid		: MSGID text						{ Sink.SetMsgId($2); }
			;

p_msgid		: P_MSGID text						{ Sink.SetMsgId($2); }
			;

o_msgid		: O_MSGID text						{ Sink.SetMsgId($2); }
			;

msgid_plural
			: MSGID_PLURAL text					{ Sink.SetMsgIdPlural($2); }
			;

msgstr		: MSGSTR text						{ Sink.SetMsgStr($2); }
			;

msgstr_plural_list
			: msgstr_plural
			| msgstr_plural_list msgstr_plural
			;

msgstr_plural
			: MSGSTR OB NUMBER CB text			{ Sink.SetMsgStrPlural($3, $5); }
			;

text		: STRING
			| text STRING						{ $$ = $1 + $2; }
			;
