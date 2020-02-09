<?xml version="1.0" encoding="utf-8"?>
<xsl:stylesheet version="1.0" xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
                xmlns:disfr="http://github.com/AlissaSabre/disfr/"
                exclude-result-prefixes="disfr"
                >

  <!-- Note that indent="yes" doesn't work well with white-space: pre-wrap on some XML processors. -->
  <xsl:output method="html" indent="yes" encoding="utf-8"/>

  <!-- Note that all spaces in the STYLE content are removed.  You should avoid using them. -->
  <xsl:variable name="STYLE">
    body   { color: black; background: white; font-family: sans-serif; }
    table  { border-collapse: collapse; }
    th, td { border-style: solid; border-width: thin; border-color: gray; }
    th     { background: silver; }
    ins    { background: lightgreen; text-decoration: none; }
    del    { background: lightpink;  text-decoration: line-through; }
    .tag   { color: maroon; }
    .chg   { background: yellow; }
    span, ins, del { white-space: pre-wrap; }
  </xsl:variable>
  
  <xsl:template match="/">
    <!-- http://refluxflow.net/2014/07/xslt-html5.html#ID52C59285 by refluxflow@gmail.com -->
    <xsl:text disable-output-escaping="yes"><![CDATA[<!DOCTYPE html>]]>&#10;</xsl:text>
    <html>
      <head>
        <title>disfr output</title>
        <style>
          <xsl:value-of select="translate(normalize-space($STYLE),' ','')"/>
        </style>
      </head>
      <body>
        <h1>disfr output</h1>
        <table>
          <thead>
            <xsl:apply-templates select="/disfr:Tree/disfr:Columns"/>
          </thead>
          <tbody>
            <xsl:apply-templates select="/disfr:Tree/disfr:Row"/>
          </tbody>
        </table>
      </body>
    </html>
  </xsl:template>

  <xsl:template match="disfr:Columns">
    <tr>
      <xsl:apply-templates select="disfr:Col"/>
    </tr>
  </xsl:template>

  <xsl:template match="disfr:Col">
    <th>
      <xsl:value-of select="."/>
    </th>
  </xsl:template>

  <xsl:template match="disfr:Row">
    <tr>
      <xsl:apply-templates select="disfr:Data"/>
    </tr>
  </xsl:template>

  <xsl:template match="disfr:Data">
    <td>
      <xsl:apply-templates select="text()"/>
    </td>
  </xsl:template>

  <xsl:template match="disfr:Data[disfr:Span]">
    <td>
      <xsl:apply-templates select="disfr:Span"/>
    </td>
  </xsl:template>

  <xsl:template match="disfr:Data[disfr:Span[@Gloss='NOR INS' or @Gloss='NOR DEL' or @Gloss='TAG INS' or @Gloss='TAG DEL']]">
    <td class="chg">
      <xsl:apply-templates select="disfr:Span"/>
    </td>
  </xsl:template>
  

  <xsl:template match="disfr:Span[@Gloss='NOR']">
    <span>
      <xsl:apply-templates select="text()"/>
    </span>
  </xsl:template>

  <xsl:template match="disfr:Span[@Gloss='NOR INS']">
    <ins>
      <xsl:apply-templates select="text()"/>
    </ins>
  </xsl:template>

  <xsl:template match="disfr:Span[@Gloss='NOR DEL']">
    <del>
      <xsl:apply-templates select="text()"/>
    </del>
  </xsl:template>

  <xsl:template match="disfr:Span[@Gloss='NOR EMP']">
    <em>
      <xsl:apply-templates select="text()"/>
    </em>
  </xsl:template>

  <xsl:template match="disfr:Span[@Gloss='TAG']">
    <span class="tag">
      <xsl:apply-templates select="text()"/>
    </span>
  </xsl:template>

  <xsl:template match="disfr:Span[@Gloss='TAG INS']">
    <ins class="tag">
      <xsl:apply-templates select="text()"/>
    </ins>
  </xsl:template>

  <xsl:template match="disfr:Span[@Gloss='TAG DEL']">
    <del class="tag">
      <xsl:apply-templates select="text()"/>
    </del>
  </xsl:template>

  <xsl:template match="disfr:Span[@Gloss='TAG EMP']">
    <em class="tag">
      <xsl:apply-templates select="text()"/>
    </em>
  </xsl:template>


</xsl:stylesheet>
