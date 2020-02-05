<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0"
                xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:disfr="http://github.com/AlissaSabre/disfr/"
                exclude-result-prefixes="disfr">
  
  <xsl:output method="xml" indent="yes"/>

  <xsl:template match="/">
    <table>
      <xsl:apply-templates select="/disfr:Tree/disfr:Row"/>
    </table>
  </xsl:template>

  <xsl:template match="disfr:Row">
    <Row>
      <xsl:apply-templates select="disfr:Data"/>
    </Row>
  </xsl:template>

  <xsl:template match="disfr:Data">
    <xsl:if test="node()">
      <xsl:element name="{translate(@Path,'[]','__')}">
        <xsl:apply-templates select="disfr:Span"/>
      </xsl:element>
    </xsl:if>
  </xsl:template>

  <xsl:template match="disfr:Span[@Gloss='ALT' or @Gloss='ALT INS' or @Gloss='ALT DEL']" />

  <xsl:template match="disfr:Span">
    <xsl:value-of select="."/>
  </xsl:template>

  <xsl:template match="node()" />
  
</xsl:stylesheet>
