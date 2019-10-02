<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"

    xmlns="urn:schemas-microsoft-com:office:spreadsheet"
    xmlns:ss="urn:schemas-microsoft-com:office:spreadsheet"
    xmlns:html="http://www.w3.org/TR/REC-html40"
    xmlns:d="http://github.com/AlissaSabre/disfr/"

    exclude-result-prefixes="d">
  
  <xsl:output method="xml" indent="no" encoding="utf-8"/>
  
  <!-- Color names as defined in CSS standard. -->
  <xsl:variable name="MAROON"     select="'#800000'" />
  <xsl:variable name="GREEN"      select="'#008000'" />
  <xsl:variable name="SILVER"     select="'#c0c0c0'" />
  <xsl:variable name="PURPLE"     select="'#800080'" />

  <!-- This is a <i>custom</i> color. -->
  <xsl:variable name="BRIGHTYELLOW" select="'#ffffaa'" />
  
  <!-- Colors assignment -->
  <xsl:variable name="CHANGED" select="$BRIGHTYELLOW" />
  <xsl:variable name="INS" select="$GREEN" />
  <xsl:variable name="DEL" select="$MAROON" />
  <xsl:variable name="TAG" select="$PURPLE" />
  <xsl:variable name="TAGINS" select="$PURPLE" />
  <xsl:variable name="TAGDEL" select="$PURPLE" />

  <xsl:template match="/d:Tree">
    <Workbook>
      
      <Styles>
        <Style ss:ID="HX">
          <Alignment ss:WrapText="0" ss:Horizontal="Left" ss:Vertical="Top" />
          <Interior ss:Pattern="None" />
        </Style>
        <Style ss:ID="TH">
          <Alignment ss:WrapText="1" ss:Horizontal="Center" ss:Vertical="Top" />
          <Borders>
            <Border ss:LineStyle="Continuous" ss:Weight="1" ss:Position="Left" />
            <Border ss:LineStyle="Continuous" ss:Weight="1" ss:Position="Top" />
            <Border ss:LineStyle="Continuous" ss:Weight="1" ss:Position="Right" />
            <Border ss:LineStyle="Continuous" ss:Weight="1" ss:Position="Bottom" />
          </Borders>
          <Interior ss:Pattern="Solid" ss:Color="{$SILVER}" />
        </Style>
        <Style ss:ID="OP">
          <Alignment ss:WrapText="1" ss:Horizontal="Left" ss:Vertical="Top" />
          <Borders>
            <Border ss:LineStyle="Continuous" ss:Weight="1" ss:Position="Left" />
            <Border ss:LineStyle="Continuous" ss:Weight="1" ss:Position="Top" />
            <Border ss:LineStyle="Continuous" ss:Weight="1" ss:Position="Right" />
            <Border ss:LineStyle="Continuous" ss:Weight="1" ss:Position="Bottom" />
          </Borders>
          <Interior ss:Pattern="None" />
        </Style>
        <Style ss:ID="CH">
          <Alignment ss:WrapText="1" ss:Horizontal="Left" ss:Vertical="Top" />
          <Borders>
            <Border ss:LineStyle="Continuous" ss:Weight="1" ss:Position="Left" />
            <Border ss:LineStyle="Continuous" ss:Weight="1" ss:Position="Top" />
            <Border ss:LineStyle="Continuous" ss:Weight="1" ss:Position="Right" />
            <Border ss:LineStyle="Continuous" ss:Weight="1" ss:Position="Bottom" />
          </Borders>
          <Interior ss:Pattern="Solid" ss:Color="{$CHANGED}" />
        </Style>
      </Styles>
    
      <Worksheet ss:Name="disfr">
        <Table>
          <xsl:apply-templates mode="COLUMN" select="d:Columns/d:Col" />
          <Row>
            <xsl:apply-templates mode="HEADING" select="d:Columns/d:Col" />
          </Row>
          <xsl:apply-templates select="d:Row" />
        </Table>
      </Worksheet>
      
    </Workbook>

  </xsl:template>
  
  <xsl:template mode="COLUMN" match="d:Col">
    <Column ss:Width="200"/>
  </xsl:template>
  
  <xsl:template mode="COLUMN" match="d:Col[@Path='Serial' or @Path='Serial2']">
    <Column ss:Width="20"/>
  </xsl:template>
  
  <xsl:template mode="COLUMN" match="d:Col[@Path='Id' or @Path='Id2']">
    <Column ss:Width="80"/>
  </xsl:template>

  <xsl:template mode="HEADING" match="d:Col">
    <Cell ss:StyleID="TH">
      <Data ss:Type="String">
        <xsl:value-of select="."/>
      </Data>
    </Cell>
  </xsl:template>
  
  <xsl:template match="d:Row">
    <Row>
      <xsl:apply-templates select="d:Data"/>
    </Row>
  </xsl:template>

  <xsl:template match="d:Data">
    <Cell ss:StyleID="OP">
      <Data ss:Type="String">
        <xsl:value-of select="."/>
      </Data>
    </Cell>
  </xsl:template>

  <xsl:template match="d:Data[@Path='Serial' or @Path='Serial2']">
    <Cell ss:StyleID="OP">
      <Data ss:Type="Number">
        <xsl:value-of select="."/>
      </Data>
    </Cell>
  </xsl:template>
    
  <xsl:template match="d:Data[d:Span]">
    <Cell ss:StyleID="CH">
      <ss:Data ss:Type="String" xmlns="http://www.w3.org/TR/REC-html40">
        <xsl:apply-templates select="d:Span"/>
      </ss:Data>
    </Cell>  
  </xsl:template>
  
  <xsl:template match="d:Span">
    <xsl:value-of select="."/>
  </xsl:template>

  <xsl:template match="d:Span[@Gloss='NOR']">
    <Font xmlns="http://www.w3.org/TR/REC-html40">
      <xsl:value-of select="."/>
    </Font>
  </xsl:template>

  <xsl:template match="d:Span[@Gloss='NOR INS']">
    <Font xmlns="http://www.w3.org/TR/REC-html40" html:Color="{$INS}">
      <U><xsl:value-of select="."/></U>
    </Font>
  </xsl:template>

  <xsl:template match="d:Span[@Gloss='NOR DEL']">
    <Font xmlns="http://www.w3.org/TR/REC-html40" html:Color="{$DEL}">
      <S><xsl:value-of select="."/></S>
    </Font>
  </xsl:template>

  <xsl:template match="d:Span[@Gloss='TAG']">
    <Font xmlns="http://www.w3.org/TR/REC-html40" html:Color="{$TAG}">
      <xsl:value-of select="."/>
    </Font>
  </xsl:template>

  <xsl:template match="d:Span[@Gloss='TAG INS']">
    <Font xmlns="http://www.w3.org/TR/REC-html40" html:Color="{$TAGINS}">
      <U><xsl:value-of select="."/></U>
    </Font>
  </xsl:template>

  <xsl:template match="d:Span[@Gloss='TAG DEL']">
    <Font xmlns="http://www.w3.org/TR/REC-html40" html:Color="{$TAGDEL}">
      <S><xsl:value-of select="."/></S>
    </Font>
  </xsl:template>

  <xsl:template match="d:Span[@Gloss='SYM']">
    <Font xmlns="http://www.w3.org/TR/REC-html40">
      <xsl:value-of select="."/>
    </Font>
  </xsl:template>

  <xsl:template match="d:Span[@Gloss='SYM INS']">
    <Font xmlns="http://www.w3.org/TR/REC-html40" html:Color="{$INS}">
      <U><xsl:value-of select="."/></U>
    </Font>
  </xsl:template>

  <xsl:template match="d:Span[@Gloss='SYM DEL']">
    <Font xmlns="http://www.w3.org/TR/REC-html40" html:Color="{$DEL}">
      <S><xsl:value-of select="."/></S>
    </Font>
  </xsl:template>

  <xsl:template match="d:Span[@Gloss='ALT' or @Gloss='ALT INS' or @GLoss='ALT DEL']" />

</xsl:stylesheet>
