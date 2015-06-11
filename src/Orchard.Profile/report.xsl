<?xml version="1.0"?>

<xsl:stylesheet
    xmlns:xsl="http://www.w3.org/1999/XSL/Transform"
    xmlns:msxsl="urn:schemas-microsoft-com:xslt"
    xmlns:ms="urn:microsoft-performance"
    exclude-result-prefixes="msxsl"
    version="1.0">
<xsl:output method='html' indent='yes' standalone='yes' encoding="utf-16"/>

<!-- ********** XSL PARAMETERS ********** -->

<xsl:param name="defaultTop"/>
<xsl:param name="defaultLevel"/>

<!--***************************************************************************

    Copyright (c) Microsoft Corporation. All rights reserved.

****************************************************************************-->

<!-- ********** XSL FUNCTIONS ********** -->

<msxsl:script language='JScript' implements-prefix='ms'><![CDATA[

var g_tag = 0;

function unique( list, index ){
  var check = index - 1;

  if( check == 0 ){
    return 1;
  }

  for( var i=0;i<check;i++ ){
    var compare = list.item(i).text;
    if( list.item(check).text == compare ){
      return 0;
    }
  }

  return 1;
}

function top( list, field )
{
    try{
        var topIndex = 0;
        var topValue = 0;
        var filter = "data[@name = '" + field + "']";

        for(i=0;i<list.length;i++){
            var node = list.item(i).selectSingleNode( filter );
            if( node ){
              var test = node.text * 1;
              if( test > topValue ){
                  topIndex = i;
                  topValue = test;
              }
            }
        }

        return (topIndex+1) * 1;

    }catch(e){
        return 0;
    }
}

function tag(){
    return ++g_tag;
}

]]></msxsl:script>

<xsl:variable name='titles'>

  <title name="contents">Contents</title>
  <title name="summary">Summary</title>
  <title name="top">Top:</title>
  <title name="topOf">of</title>
  <title name="warnings">Warnings</title>
  <title name="type">Type</title>
  <title name="item">Item</title>
  <title name="warning">Warning</title>
  <title name="help">Help</title>
  <title name="total">Total</title>
  <title name="average">Average</title>
  <title name="recordCount">Transactions: </title>
  <title name="uniqueCount">Total Records: </title>

  <xsl:for-each select="//titles/title">
    <xsl:copy-of select="."/>
  </xsl:for-each>
</xsl:variable>

<xsl:variable name='tab'><xsl:value-of select="'&#xA0;&#xA0;&#xA0;&#xA0;&#xA0;&#xA0;&#xA0;&#xA0;&#xA0;&#xA0;&#xA0;&#xA0;&#xA0;&#xA0;&#xA0;&#xA0;'"/></xsl:variable>

<xsl:variable name="Top">
  <xsl:choose>
    <xsl:when test="$defaultTop"><xsl:value-of select="$defaultTop"/></xsl:when>
    <xsl:when test="/report/@top"><xsl:value-of select="/report/@top"/></xsl:when>
    <xsl:otherwise>10</xsl:otherwise>
  </xsl:choose>
</xsl:variable>

<xsl:variable name="Level">
  <xsl:choose>
    <xsl:when test="$defaultLevel"><xsl:value-of select="$defaultLevel"/></xsl:when>
    <xsl:when test="/report/@level"><xsl:value-of select="/report/@level"/></xsl:when>
    <xsl:otherwise>1</xsl:otherwise>
  </xsl:choose>
</xsl:variable>

<xsl:template match="/">

<html>

<!--***************************************************************************

    HTML STYLE DEFINITION

****************************************************************************-->

<style>
  body{ font-family: Verdana,Arial; color: black; margin-left: 1%; margin-right: 1%; }
  td{ font-size: 75%; }
  th{ font-size: 70%; font-weight: bolder; border: 1px solid lightgrey; vertical-align: bottom; }
  hr{ border:1px solid black; }
  a{ cursor:hand; }
  a:link{ color: black; text-decoration: underline; }
  a:visited{ color: black; text-decoration: underline; }
  li { font-size: 130%; }
  li li { font-size: 75%; }
  li li li { font-size: 90%; }

  .block{ border: solid black 1px; width: 100%; }
  .thin{ border:1px solid black; height:1px;}
  .popup{ position:absolute; z-index:1; background-color:infobackground; border:solid; border-width:1px; border-right-width:2px; border-bottom-width:2px; font-size: x-small;font-weight: normal;text-align: left;padding: 8px; }

  .h1{ font-size: 110%; font-weight: bolder; }
  .h2{ font-size: 105%; font-weight: bolder; }
  .h3{ font-size: 80%; font-weight: bolder; }
  .h4{ font-size: 60%; font-weight: bolder; }

  .b1{ background: white; }
  .b2{ background: whitesmoke; }
  .b3{ background: lightgrey; }
  .b4{ background: gray; }

  .bold{ font-weight: bolder; }
  .italic{ font-style: italic; }

  .number{ text-align: right; }
  .string{ text-align: left; }
  .info{ font-size: 60%; }
  .icon{ vertical-align: center; text-align: center; font-family: webdings; font-size: 20pt; }
  .code{ font-family: courier; }
  .span{ text-align: center; border-bottom:1px solid lightgrey;}

  .total{ font-style: normal; }
  .average{ font-style: italic; }
</style>

<body onload='onload();' class='b1'>
<form>

<!--***************************************************************************

   RUNTIME SCRIPT

****************************************************************************-->

<script>

function onload()
{
    <xsl:for-each select="//include">
        <xsl:value-of select="document( @document )//@init"/>
    </xsl:for-each>
}

function popup( d )
{

    d.style.display = '';
    var x = window.event.x + 12;

    if(  d.clientWidth + x > (document.body.clientWidth-4) ){
        d.style.left =  window.event.x - 8 - d.clientWidth;
        d.style.right = window.event.x - 8;
    }else{
        d.style.left =  x;
        d.style.right = x + d.clientWidth;
    }
}

function help( url )
{
    showHelp( url );
}

var cc = "";

function compare(elem1, elem2, reverse)
{
  var sgn = reverse ? -1 : 1;

  if (elem1.isnum &amp;&amp; !elem2.isnum){
    return -1;
  }
  if (elem2.isnum &amp;&amp; !elem1.isnum){
    return 1;
  }
  if (elem1.text &lt; elem2.text){
    return -sgn;
  }
  if (elem1.text > elem2.text){
    return sgn;
  }
  return 0;
}

function sort( t )
{
  try{
    var tbody = t.tBodies(0);
    var iColumn = window.event.srcElement.cellIndex;
    var reverse;
    var iRowEnd = tbody.rows.length-1;
    var iSortRowCnt;

    for( var col = 0; col &lt; iColumn; col++ ){
      if( tbody.rows[0].cells[col].colSpan > 1 ){
        iColumn -= (tbody.rows[0].cells[col].colSpan - 1);
      }
    }

    var key = t.id + "_" + iColumn;

    if( isNaN( tbody.children[0].children[iColumn].innerText.charAt(0) ) ){
      reverse = false;
    }else{
      reverse = true;
    }

    if( cc == key ){
      cc = "";
      reverse = !reverse;
    }else{
      cc = key;
    }

    var t1 = new Array();
    var t2 = new Array();
    var tab1, tab2;
    var i, j;
    var re =/\D/g;

    iSortRowCnt = 0;
    for (i = 0; i &lt;= iRowEnd; ++i){
      if (tbody.children[i].child == 'true'){
        continue;
      }

      t1[iSortRowCnt] = new Object();
      if (typeof(tbody.children[i].children[iColumn]) != "undefined"){
        text = tbody.children[i].children[iColumn].innerText;
      }else{
        text = "";
      }

      if (!isNaN(text.charAt(0))){
        t1[iSortRowCnt].text = eval(text.replace(re, ""));
        t1[iSortRowCnt].isnum = true;
      }else{
        t1[iSortRowCnt].text = text.toLowerCase();
        t1[iSortRowCnt].isnum = false;
      }
      t1[iSortRowCnt].ptr = tbody.children[i];
      iSortRowCnt++;
    }

    tab2 = t1;
    tab1 = t2;
    for (var iSize = 1; iSize &lt; iSortRowCnt; iSize *= 2){
      var iBeg, iLeft, iRight, iLeftEnd, iRightEnd, iDest;

      if (tab1 == t2) {
        tab1 = t1;
        tab2 = t2;
      }else{
        tab1 = t2;
        tab2 = t1;
      }

      for (iBeg = 0; iBeg &lt; iSortRowCnt; iBeg += iSize*2){
        iRight = iBeg+iSize;

        if (iRight >= iSortRowCnt){
          break;
        }

        iRightEnd = iRight+iSize-1;

        if (iRightEnd >= iSortRowCnt){
          iRightEnd = iSortRowCnt-1;
        }

        iLeftEnd = iRight-1;
        iLeft = iBeg;

        for (iDest = iLeft; iDest &lt;= iRightEnd; ++iDest){
          if ((iRight > iRightEnd) ||
            (iLeft &lt;= iLeftEnd &amp;&amp;
            compare(tab1[iLeft], tab1[iRight], reverse) &lt;= 0) ){
            tab2[iDest] = tab1[iLeft];
            ++iLeft;
          }else{
            tab2[iDest] = tab1[iRight];
            ++iRight;
          }
        }
      }

      for (iDest = iRightEnd+1; iDest &lt; iSortRowCnt; ++iDest){
        tab2[iDest] = tab1[iDest];
      }
    }

    for (i = iSortRowCnt-1; i >= 0; --i){
      var first = tbody.children[0];
      var insert = tab2[i].ptr, next;

      if (insert == first) {
        continue;
      }

      next = insert.nextSibling;
      while ( next &amp;&amp; next.child == 'true' ) {
        tbody.insertBefore(insert, first);
        insert = next;
        next = insert.nextSibling;
      }
      tbody.insertBefore(insert, first);
    }

  }catch(e){
  }

  if( tbody.mode == "child" ){
    shade( tbody );
  }else{
    show( t );
  }

}

function show( t )
{
  var tbody = t.tBodies(0);
  var top = document.all("top_"+t.id).value;
  var count = 0;
  var display = 'none';
  var visible = 0;
  var children = 0;
  var childDisplay = 'none';

  for( var i=0; i&lt;tbody.rows.length;i++ ){

    if( tbody.children[i].child != 'true' ){
      children = 0;
      if( count++ &lt; top ){
        display = '';
        visible++;
      }else{
        display = 'none';
      }
      if( tbody.rows[i].cells[0].innerText == '+' || display == 'none' ){
        childDisplay = 'none';
      }else{
        childDisplay = '';
      }
    }else{
      if( children++ > top ){
        display = 'none';
      }else{
        display = childDisplay;
      }
    }

    tbody.children[i].style.display = display;

    try{
      document.all( t.id + "_child_" + tbody.children[i].index ).style.display = display;
    }catch(e){
    }
  }

  document.all("top_"+t.id).value = visible;

  shade( tbody );
}

function shade( tbody ){
  var p = 0;
  var light;
  var dark;

  light = "b1";
  dart = "b3";

  for ( i = 0; i &lt; tbody.rows.length; i++){
    if( tbody.children[i].style.display == '' ){
      if( tbody.children[i].child != 'true' ){
        p++;
      }
      if( p % 2 == 0 ){
        className = light;
      }else{
        className = dart;
      }
      if( tbody.children[i].child != 'true' ){
        tbody.children[i].className = className;
      }
    }
  }
}

function pressTop( t ){
  if( window.event.keyCode == 13 ){
    show( t );
    window.event.returnValue = false;
  }
}

function folder( d ){
  try{
    e = document.all( "e_" + d.id );
    if( d.style.display == 'none' ){
      d.style.display = '';
      e.innerText = "-";
    }else{
      d.style.display = 'none';
      e.innerText = "+";
    }
  }catch(e){
    alert( e.message );
  }
}

<xsl:for-each select="//include">
    <xsl:value-of select="document( @document )//script"/>
</xsl:for-each>

</script>

<!--***************************************************************************

   REPORT

****************************************************************************-->

<xsl:for-each select="/report[@name]">
  <table class="block" cellpadding="2">
    <tr>
      <td class="h1">
        <xsl:call-template name='title'/>
      </td>
    </tr>
    <xsl:if test="count(//data[@header])">
      <tr><td><hr/></td></tr>
      <tr>
        <td>
          <table>
            <xsl:for-each select="//data[@header]">
              <tr>
                <td class="h4">
                  <xsl:call-template name="label">
                    <xsl:with-param name="label" select="@header"/>
                  </xsl:call-template>:
                </td>
                <td class="info">
                  <xsl:call-template name="data"/>
                  <xsl:apply-templates select="@warning"/>
                  <xsl:apply-templates select="@note"/>
                </td>
              </tr>
            </xsl:for-each>
          </table>
        </td>
      </tr>
    </xsl:if>
  </table>
  <br/>
</xsl:for-each>

<!-- ********** TABLE OF CONTENTS ********** -->

<table class="block">
  <tr><td valign='top'>

  <table width='100%'>
  <tr>
    <td valign='top' class='h2'>
      <a name="contents" accesskey="c" tabindex="1">
        <xsl:call-template name="label">
          <xsl:with-param name="label" select="'contents'"/>
        </xsl:call-template>
      </a>
      <hr/>
    </td>
  </tr><tr>
    <td>
      <ul>
        <xsl:if test="//@warning and not(report/section[@name='advice'])">
          <li>
            <font class="bold">
              <a href='#Warnings'>
                <xsl:call-template name="label">
                  <xsl:with-param name="label" select="'warnings'"/>
                </xsl:call-template>
              </a>
            </font>
          </li>
          <br/>
          <br/>
        </xsl:if>

        <xsl:for-each select="report/section[(table[@level &lt;= $Level or not(@level)][item[@level &lt;= $Level or not(@level)][not(@visible='false')]][ not( @visible='false') and not(@section) ] or //table[@level &lt;= $Level or not(@level)][item[@level &lt;= $Level or not(@level)][not(@visible='false')]]/@section=@name ) or (@name='advise' and //@warning)]">
        <xsl:sort select="not(@key) or @key &lt; 0" data-type="number"/>
        <xsl:sort select="@key" data-type="number"/>
          <li>
            <font class="bold">
              <xsl:call-template name='title'>
                <xsl:with-param name='nosub' select="1"/>
              </xsl:call-template>
            </font>
          </li>
          <ul>
            <xsl:if test="//@warning and @name='advice'">
              <li>
                <a href='#Warnings'>
                  <xsl:call-template name="label">
                    <xsl:with-param name="label" select="'warnings'"/>
                  </xsl:call-template>
                </a>
              </li>
            </xsl:if>

            <xsl:variable name="tables" select="table[@level &lt;= $Level or not(@level)][item[@level &lt;= $Level or not(@level)][not(@visible='false')]][not(@visible='false') and not(@section)] | //table[@level &lt;= $Level or not(@level)][item[@level &lt;= $Level or not(@level)][not(@visible='false')]][@section=current()/@name]"/>

            <xsl:variable name="topics">
              <xsl:for-each select="$tables[@topic]">
              <xsl:sort select="not(@key) or @key &lt; 0" data-type="number"/>
              <xsl:sort select="@key" data-type="number"/>
                <xsl:element name="topic">
                  <xsl:value-of select="@topic"/>
                </xsl:element>
              </xsl:for-each>
            </xsl:variable>

            <xsl:for-each select="msxsl:node-set($topics)/topic">
              <xsl:if test="ms:unique( msxsl:node-set($topics)/topic, number(position()) )">
                <xsl:variable name="topic" select="."/>
                <li>
                  <font class="bold">
                    <xsl:call-template name="label">
                      <xsl:with-param name="label" select="$topic"/>
                    </xsl:call-template>
                  </font>
                </li>
                <ul>
                  <xsl:for-each select="$tables[@topic=$topic]">
                  <xsl:sort select="not(@key) or @key &lt; 0" data-type="number"/>
                  <xsl:sort select="@key" data-type="number"/>
                    <li>
                      <a>
                      <xsl:attribute name='href'>#<xsl:value-of select='@name'/></xsl:attribute>
                        <xsl:call-template name='title'>
                          <xsl:with-param name='nosub' select="1"/>
                        </xsl:call-template>
                      </a>
                    </li>
                  </xsl:for-each>
                </ul>
              </xsl:if>

            </xsl:for-each>

            <xsl:for-each select="$tables[not(@topic)]">
            <xsl:sort select="not(@key) or @key &lt; 0" data-type="number"/>
            <xsl:sort select="@key" data-type="number"/>
              <li>
                <a>
                <xsl:attribute name='href'>#<xsl:value-of select='@name'/></xsl:attribute>
                  <xsl:call-template name='title'>
                    <xsl:with-param name='nosub' select="1"/>
                  </xsl:call-template>
                </a>
              </li>
            </xsl:for-each>
          </ul>
          <br/>
        </xsl:for-each>
      </ul>
    </td>
  </tr>
  </table>
  </td>

<!-- ********** SUMMARY SECTION ********** -->

  <xsl:if test="//table[summary]">
  <td valign='top'>
  <table width='100%'>
  <tr>
    <td class='h2'>
      <xsl:call-template name="label">
        <xsl:with-param name="label" select="'summary'"/>
      </xsl:call-template>
      <hr/>
    </td>
  </tr>
  <xsl:for-each select="//table[summary]/summary">
  <xsl:sort select="not(@key) or @key &lt; 0" data-type="number"/>
  <xsl:sort select="@key" data-type="number"/>
  <tr>
    <td>
    <xsl:variable name="summaryTable"><xsl:value-of select="parent::table/@name"/></xsl:variable>
    <table class="block" cellspacing='3px'>
    <tr>
      <xsl:for-each select="data">
        <td>
        <xsl:attribute name='class'><xsl:value-of select='@class'/> bold</xsl:attribute>
          <xsl:choose>
            <xsl:when test='position() = 1'>
            <a>
            <xsl:attribute name='href'>#<xsl:value-of select='$summaryTable'/></xsl:attribute>
              <xsl:call-template name='title'/>
            </a>
            </xsl:when>
            <xsl:otherwise>
              <xsl:call-template name='title'/>
            </xsl:otherwise>
          </xsl:choose>
        </td>
      </xsl:for-each>
    </tr>
    <tr>
      <xsl:choose>
        <xsl:when test="@find='top'">
          <xsl:variable name='summaryIndex'><xsl:value-of select="ms:top(parent::table/item[(data[@name=current()/@exclude] != current()/@value) or not(current()/@exclude)],string(@field))"/></xsl:variable>
          <xsl:for-each select="data">
            <td>
            <xsl:attribute name="class"><xsl:value-of select="@class"/><xsl:value-of select="'&#x20;'"/>b3</xsl:attribute>
              <xsl:variable name="format">
                <xsl:choose>
                  <xsl:when test="@format"><xsl:value-of select="@format"/></xsl:when>
                </xsl:choose>
              </xsl:variable>
              <xsl:if test="position() != 1">
                <xsl:attribute name='width'>40%</xsl:attribute>
              </xsl:if>
              <xsl:for-each select="/report/section/table[@name=$summaryTable]/item[data[@name=current()/parent::summary/@exclude] != current()/parent::summary/@value or not(current()/parent::summary/@exclude)][number($summaryIndex)]/data[@name=current()/@name]">
                <xsl:call-template name="data">
                  <xsl:with-param name="format" select="$format"/>
                </xsl:call-template>
              </xsl:for-each>
              <xsl:apply-templates select="/report/section/table[@name=$summaryTable]/item[data[@name=current()/parent::summary/@exclude] != current()/parent::summary/@value or not(current()/parent::summary/@exclude)][number($summaryIndex)]/data[@name=current()/@name]/@warning"/>
              <xsl:apply-templates select="/report/section/table[@name=$summaryTable]/item[data[@name=current()/parent::summary/@exclude] != current()/parent::summary/@value or not(current()/parent::summary/@exclude)][number($summaryIndex)]/data[@name=current()/@name]/@note"/>
            </td>
          </xsl:for-each>
        </xsl:when>
        <xsl:when test="@find='average'">
          <td>
          <xsl:attribute name='class'><xsl:value-of select="data/@class"/><xsl:value-of select="'&#x20;'"/>b3</xsl:attribute>
            <xsl:variable name="format">
              <xsl:choose>
                <xsl:when test="data/@format"><xsl:value-of select="data/@format"/></xsl:when>
                <xsl:otherwise>0.00</xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:value-of select="format-number( sum(/report/section/table[@name=$summaryTable]/item[data[@name=current()/@exclude] != current()/@value or not(current()/parent::summary/@exclude)]/data[@name=current()/@field]) div count(/report/section/table[@name=$summaryTable]/item/data[@name=current()/@field]), $format)"/>
          </td>
        </xsl:when>
        <xsl:when test="@find='total'">
          <td>
          <xsl:attribute name="class"><xsl:value-of select="data/@class"/><xsl:value-of select="'&#x20;'"/>b3</xsl:attribute>
            <xsl:variable name="format">
              <xsl:choose>
                <xsl:when test="data/@format"><xsl:value-of select="data/@format"/></xsl:when>
                <xsl:otherwise>0.00</xsl:otherwise>
              </xsl:choose>
            </xsl:variable>
            <xsl:value-of select="format-number( sum(/report/section/table[@name=$summaryTable]/item[data[@name=current()/@exclude] != current()/@value]/data[@name=current()/@field]), $format )"/>
          </td>
        </xsl:when>
        <xsl:when test="@find='field'">
          <xsl:variable name='field'><xsl:value-of select='@field'/></xsl:variable>
          <xsl:variable name='value'><xsl:value-of select='@value'/></xsl:variable>

          <xsl:for-each select="data">
          <td>
            <xsl:variable name="format">
              <xsl:choose>
                <xsl:when test="@format"><xsl:value-of select="@format"/></xsl:when>
              </xsl:choose>
            </xsl:variable>
            <xsl:attribute name="class"><xsl:value-of select="@class"/><xsl:value-of select="'&#x20;'"/>b3</xsl:attribute>
            <xsl:if test="position() != 1">
              <xsl:attribute name='width'>40%</xsl:attribute>
            </xsl:if>

              <xsl:for-each select="/report/section/table[@name=$summaryTable]/item[data[@name=$field]=$value]/data[@name=current()/@name]">
                <xsl:call-template name="data">
                  <xsl:with-param name="format" select="$format"/>
                </xsl:call-template>
              </xsl:for-each>
              <xsl:apply-templates select="/report/section/table[@name=$summaryTable]/item[data[@name=$field]=$value]/data[@name=current()/@name]/@warning"/>
              <xsl:apply-templates select="/report/section/table[@name=$summaryTable]/item[data[@name=$field]=$value]/data[@name=current()/@name]/@note"/>
          </td>
          </xsl:for-each>
        </xsl:when>
      </xsl:choose>
    </tr>
    </table>
    </td>
  </tr>
  </xsl:for-each>
  </table>
  </td>
  </xsl:if>
  </tr>
</table>
<br/>

<!-- ********** WARNING SECTION ********** -->

<xsl:if test="//@warning and not(report/section[@name='advice'])">
  <xsl:call-template name="warning"/>
</xsl:if>

<!-- ********** SECTION HEADER ********** -->

<xsl:for-each select="report/section[(table[item[@level &lt;= $Level or not(@level)][not(@visible='false')]][ not( @visible='false') and not(@section) ] or //table[item[@level &lt;= $Level or not(@level)][not(@visible='false')]]/@section=@name ) or (@name='advise' and //@warning) and (@level > $Level or not(@level))]">
<xsl:sort select="not(@key) or @key &lt; 0" data-type="number"/>
<xsl:sort select="@key" data-type="number"/>

<table class="block" style="page-break-before:always" cellpadding='2' cellspacing='5'>
  <tr>
    <td>
    <xsl:attribute name="class">b3</xsl:attribute>
      <table width='100%'>
        <tr>
          <td class='h1'>
            <xsl:call-template name='title'/>

            <xsl:apply-templates select="@warning"/>
            <xsl:apply-templates select="@note"/>

          </td>
          <td align='right'>
            <a href="#top">
            <font style='text-decoration:none;' face="webdings" size="x-small">
            <xsl:value-of select="'&#x35;'"/>
            </font>
            </a>
          </td>
        </tr>
      </table>
    </td>
  </tr>
</table>
<br/>

<!-- ********** WARNING SECTION ********** -->

<xsl:if test="@name='advice'">
  <xsl:call-template name="warning"/>
</xsl:if>

<!-- ********** TABLE ********** -->

<xsl:variable name="tables" select="table[item[@level &lt;= $Level or not(@level)][not(@visible='false')]][not(@visible = 'false') and not(@section) and (@level &lt;= $Level or not(@level))] | //table[item[@level &lt;= $Level or not(@level)][not(@visible='false')]][@section=current()/@name and (@level &lt;= $Level or not(@level))]"/>

<xsl:variable name="topics">
  <xsl:for-each select="$tables[@topic]">
  <xsl:sort select="not(@key) or @key &lt; 0" data-type="number"/>
  <xsl:sort select="@key" data-type="number"/>
    <xsl:element name="topic">
      <xsl:value-of select="@topic"/>
    </xsl:element>
  </xsl:for-each>
</xsl:variable>

<xsl:for-each select="msxsl:node-set($topics)/topic">
  <xsl:if test="ms:unique( msxsl:node-set($topics)/topic, number(position()) )">
    <xsl:variable name="topic" select="."/>
    <table width="100%" style="border: solid gray 1px;">
      <tr><td>
        <table width="100%"><tr>
          <td class="h2 b2">
            <xsl:call-template name="label">
              <xsl:with-param name="label" select="$topic"/>
            </xsl:call-template>
          </td>
        </tr></table>
        </td>
      </tr>
    </table>
    <br/>
    <xsl:for-each select="$tables[@topic=$topic]">
    <xsl:sort select="not(@key) or @key &lt; 0" data-type="number"/>
    <xsl:sort select="@key" data-type="number"/>
        <xsl:apply-templates select="."/>
    </xsl:for-each>
  </xsl:if>

</xsl:for-each>

<xsl:for-each select="$tables[not(@topic)]">
<xsl:sort select="not(@key) or @key &lt; 0" data-type="number"/>
<xsl:sort select="@key" data-type="number"/>
  <xsl:apply-templates select="."/>
</xsl:for-each>

</xsl:for-each>

</form>
</body>
</html>
</xsl:template>


<!-- ********** TABLE TEMPLATES ********** -->

<xsl:template match="table">

<xsl:variable name="table"><xsl:value-of select="@name"/></xsl:variable>
<xsl:variable name="tableId">table_<xsl:value-of select="ms:tag()"/></xsl:variable>
<a><xsl:attribute name="name"><xsl:value-of select="@name"/></xsl:attribute></a>

<!-- ********** SORT SETUP ********** -->


  <xsl:variable name="count">
    <xsl:call-template name="itemCount"/>
  </xsl:variable>

  <xsl:variable name="totalCount">
    <xsl:choose>
      <xsl:when test="@count">
        <xsl:value-of select="@count"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="count(item[not(@visible='false')])"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="show">
    <xsl:call-template name="show">
      <xsl:with-param name="header" select="header"/>
    </xsl:call-template>
  </xsl:variable>


<!-- ********** TABLE TITLE ********** -->

<table class="block" cellspacing='3px'>
  <tr>
    <td>
      <table width='100%'>
        <tr>
          <td class='h2'>
            <xsl:call-template name='title'/>

            <xsl:apply-templates select="@warning"/>
            <xsl:apply-templates select="@note"/>

          </td>
          <td align='right' valign='top'>
            <xsl:if test="not(@style)">
              <b>
                <xsl:call-template name="label">
                  <xsl:with-param name="label" select="'top'"/>
                </xsl:call-template>
              </b>
              <xsl:value-of select="'&#xA0;'"/>

              <input type='text' size='3' class='b1' style="margin:1px;border: none; font-size: 100%;font-family: Verdana,Arial;">
              <xsl:attribute name='id'>top_<xsl:value-of select='$tableId'/></xsl:attribute>
              <xsl:attribute name='onchange'>show(<xsl:value-of select='$tableId'/>)</xsl:attribute>
              <xsl:attribute name='onkeypress'>pressTop(<xsl:value-of select='$tableId'/>)</xsl:attribute>
              <xsl:attribute name='value'>
                <xsl:value-of select="$show"/>
              </xsl:attribute>
              </input>
              <b>
                <xsl:call-template name="label">
                  <xsl:with-param name="label" select="'topOf'"/>
                </xsl:call-template>
              </b>
              <xsl:value-of select="'&#xA0;&#xA0;&#xA0;'"/>
              <xsl:value-of select="$count"/>
              <xsl:if test="$count &lt; $totalCount">
                <xsl:call-template name="note">
                  <xsl:with-param name="text">
                    <div nowrap="true">
                      <font class="bold">
                        <xsl:call-template name="label">
                          <xsl:with-param name="label" select="'uniqueCount'"/>
                        </xsl:call-template>
                      </font>
                    <xsl:value-of select="$totalCount"/>
                    </div>
                  </xsl:with-param>
                  <xsl:with-param name="width" select="'50'"/>
                </xsl:call-template>
              </xsl:if>
              <xsl:value-of select="'&#xA0;&#xA0;&#xA0;&#xA0;&#xA0;'"/>
            </xsl:if>
            <a href="#top">
              <font style='text-decoration:none;' face="webdings" size="x-small">
              <xsl:value-of select="'&#x35;'"/>
              </font>
            </a>
          </td>
        </tr>
      </table>
      <hr/>
    </td>
  </tr>

<!-- ********** INLUDE AREA ********** -->

  <xsl:for-each select='include'>
  <tr>
    <td>
    <xsl:copy-of select='document(@document)//include[@name=current()/@name]'/>
    </td>
  </tr>
  </xsl:for-each>

<!-- ********** TABLE BODY ********** -->

  <tr>
    <td>
      <xsl:choose>
        <xsl:when test="@style='info'">
          <xsl:call-template name="infoTable">
            <xsl:with-param name="Level" select="$Level"/>
            <xsl:with-param name="tableId" select="$tableId"/>
          </xsl:call-template>
        </xsl:when>
        <xsl:otherwise>
          <xsl:call-template name="colTable">
            <xsl:with-param name="header" select="header"/>
            <xsl:with-param name="id" select="$tableId"/>
            <xsl:with-param name="show" select="$show"/>
          </xsl:call-template>
        </xsl:otherwise>
      </xsl:choose>
    </td>
  </tr>
</table>

<br/><br/>


</xsl:template>

<xsl:template name="show">
<xsl:param name="header"/>

  <xsl:variable name="count">
    <xsl:call-template name="itemCount"/>
  </xsl:variable>
  <xsl:variable name="minField">
    <xsl:choose>
    <xsl:when test="$header/threshold[@type='min']"><xsl:value-of select="$header/threshold[@type='min']/@field"/></xsl:when>
    <xsl:otherwise><xsl:value-of select="item[1]/data[1]/@name"/></xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="min">
    <xsl:choose>
    <xsl:when test="$header/threshold[@type='min']"><xsl:value-of select="$header/threshold[@type='min']/@value"/></xsl:when>
    <xsl:otherwise>all</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="octaveField">
    <xsl:choose>
    <xsl:when test="$header/threshold[@type='octave']"><xsl:value-of select="$header/threshold[@type='octave']/@field"/></xsl:when>
    <xsl:otherwise><xsl:value-of select="item[1]/data[1]/@name"/></xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="octave">
    <xsl:choose>
    <xsl:when test="$header/threshold[@type='octave']">
      <xsl:variable name='octaveIndex'><xsl:value-of select="ms:top(item, string($octaveField))"/></xsl:variable>
      <xsl:value-of select="item[number($octaveIndex)]/data[@name=$octaveField] div 2"/>
    </xsl:when>
    <xsl:otherwise>all</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="top">
    <xsl:choose>
    <xsl:when test="$header/threshold[@type='top']"><xsl:value-of select="$header/threshold[@type='top']/@value"/></xsl:when>
    <xsl:otherwise><xsl:value-of select="$Top"/></xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="octaveCount">
    <xsl:choose>
      <xsl:when test="$octave = 'all'">0</xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="count(item[@level &lt;= $Level or not(@level)][not(@visible='false')][data[@name = $octaveField] >= $octave])"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="minCount">
    <xsl:choose>
      <xsl:when test="$min = 'all'">0</xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="count(item[@level &lt;= $Level or not(@level)][not(@visible='false')][data[@name = $minField] >= $min])"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="show">
    <xsl:choose>
      <xsl:when test="$octaveCount > $top and $octaveCount >= $minCount">
        <xsl:value-of select="$octaveCount"/>
      </xsl:when>
      <xsl:when test="$minCount > $top and $minCount > $octaveCount">
        <xsl:value-of select="$minCount"/>
      </xsl:when>
      <xsl:when test="$top = 'all' or ($top != 'all' and $top > $count)">
        <xsl:value-of select="$count"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$top"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:value-of select="$show"/>

</xsl:template>


<xsl:template name="itemCount">
    <xsl:value-of select="count(item[@level &lt;= $Level or not(@level)][not(@visible='false')])"/>
</xsl:template>

<xsl:template name="colTable">
<xsl:param name="mode"/>
<xsl:param name="id"/>
<xsl:param name="header"/>
<xsl:param name="show"/>

  <xsl:variable name="max">
    <xsl:choose>
      <xsl:when test="string-length($show)">all</xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="show">
          <xsl:with-param name="header" select="$header"/>
        </xsl:call-template>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="tableId">
    <xsl:choose>
      <xsl:when test="string-length($id)"><xsl:value-of select="$id"/></xsl:when>
      <xsl:otherwise>table_<xsl:value-of select="ms:tag()"/></xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="order1">
    <xsl:choose>
    <xsl:when test="$header/sort[1]/@order"><xsl:value-of select="$header/sort[1]/@order"/></xsl:when>
    <xsl:otherwise>ascending</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="type1">
    <xsl:choose>
    <xsl:when test="$header/sort[1]/@type"><xsl:value-of select="$header/sort[1]/@type"/></xsl:when>
    <xsl:otherwise>number</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="order2">
    <xsl:choose>
    <xsl:when test="$header/sort[2]/@order"><xsl:value-of select="$header/sort[2]/@order"/></xsl:when>
    <xsl:otherwise>ascending</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>
  <xsl:variable name="type2">
    <xsl:choose>
    <xsl:when test="$header/sort[2]/@type"><xsl:value-of select="$header/sort[2]/@type"/></xsl:when>
    <xsl:otherwise>number</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <table class="block" style="table-layout:auto" >
  <xsl:attribute name='id'><xsl:value-of select="$tableId"/></xsl:attribute>

  <thead style="display: table-header-group">
    <xsl:for-each select="$header">
      <tr>
        <xsl:if test="header">
          <td width="1%"/>
        </xsl:if>
        <xsl:apply-templates select="data[@level &lt;= $Level or not(@level)]" mode="header">
          <xsl:with-param name="tableId" select="$tableId"/>
          <xsl:with-param name="sort"><xsl:if test='position()=last()'>true</xsl:if></xsl:with-param>
        </xsl:apply-templates>
      </tr>
    </xsl:for-each>
  </thead>

  <tbody>
  <xsl:if test="string-length($mode)">
    <xsl:attribute name='mode'><xsl:value-of select="$mode"/></xsl:attribute>
  </xsl:if>
    <xsl:for-each select="item[ ($max = 'all' or position() &lt;= $max) and not(@visible='false') and (@level &lt;= $Level or not(@level))]">
    <xsl:sort select="data[@name=$header/sort[1]/@field]" order="{$order1}" data-type='{$type1}'/>
    <xsl:sort select="data[@name=$header/sort[2]/@field]" order="{$order2}" data-type='{$type2}'/>

      <xsl:variable name='background'>
        <xsl:choose>
        <xsl:when test="position() mod 2 = 1">b3</xsl:when>
        <xsl:otherwise>b1</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <xsl:variable name='display'>
        <xsl:choose>
          <xsl:when test="not($show)"></xsl:when>
          <xsl:when test="position() &lt;= number($show)">display:''</xsl:when>
          <xsl:otherwise>display:'none'</xsl:otherwise>
        </xsl:choose>
      </xsl:variable>

      <tr>
      <xsl:attribute name="class"><xsl:value-of select="$background"/></xsl:attribute>
      <xsl:attribute name="index"><xsl:value-of select="position()"/></xsl:attribute>
      <xsl:attribute name="style"><xsl:value-of select="$display"/></xsl:attribute>
        <xsl:variable name="rowId" select="generate-id(.)"/>
        <xsl:variable name="state">
          <xsl:choose>
            <xsl:when test="item/@expand='true'">-</xsl:when>
            <xsl:otherwise>+</xsl:otherwise>
          </xsl:choose>
        </xsl:variable>
        <xsl:choose>
          <xsl:when test="item[not(@expand='none')] and $header/header">
            <td>
              <a style="cursor:hand;text-decoration:none;">
              <xsl:attribute name="onclick">folder( c_<xsl:value-of select="$rowId"/> )</xsl:attribute>
                <xsl:call-template name="expand">
                  <xsl:with-param name="id">e_c_<xsl:value-of select="$rowId"/></xsl:with-param>
                  <xsl:with-param name="state" select="$state"/>
                </xsl:call-template>
              </a>
            </td>
          </xsl:when>
          <xsl:when test="$header/header"><td/>
          </xsl:when>
        </xsl:choose>

        <xsl:apply-templates select="data[not(@visible = 'false') and (@level &lt;= $Level or not(@level))]">
          <xsl:with-param name="header" select="$header"/>
        </xsl:apply-templates>

      </tr>

      <xsl:if test="item[not(@expand='none')] and $header/header">
        <tr child="true">
        <xsl:attribute name="id">c_<xsl:value-of select="$rowId"/></xsl:attribute>
        <xsl:attribute name="style">
          <xsl:choose>
            <xsl:when test="$state='-'"><xsl:value-of select="$display"/></xsl:when>
            <xsl:otherwise>display:'none';</xsl:otherwise>
          </xsl:choose>
        </xsl:attribute>
          <td colspan='100'>
            <table width="100%" cellpadding="0" cellspacing="0">
              <tr>
                <td width="10px"/>
                <td>
                  <xsl:call-template name="colTable">
                    <xsl:with-param name="header" select="$header/header"/>
                    <xsl:with-param name="mode">child</xsl:with-param>
                  </xsl:call-template>
                </td>
              </tr>
            </table>
          </td>
        </tr>
      </xsl:if>

    </xsl:for-each>
  </tbody>

  <xsl:if test="$header/data[@summary]">
    <tfoot>
        <tr>

          <xsl:variable name="items" select="item"/>

          <xsl:if test="$header/header">
            <td width="1%"/>
          </xsl:if>

          <xsl:for-each select="$header[data[@summary]]/data">
            <td>
            <xsl:if test="@summary">
              <xsl:attribute name="style">border-top: 1px solid black;</xsl:attribute>
            </xsl:if>
            <xsl:attribute name="colspan"><xsl:value-of select="@span"/></xsl:attribute>
            <xsl:attribute name="class"><xsl:value-of select="@class"/></xsl:attribute>

              <xsl:variable name='format'>
                <xsl:choose>
                  <xsl:when test="@format"><xsl:value-of select="@format"/></xsl:when>
                  <xsl:otherwise>0</xsl:otherwise>
                </xsl:choose>
              </xsl:variable>

              <xsl:if test='@summary'>
                <xsl:variable name='summary'>
                  <xsl:choose>
                  <xsl:when test="string-length(.)">
                    <xsl:value-of select="format-number( ., $format )"/>
                  </xsl:when>
                  <xsl:when test="@summary='total'">
                    <xsl:value-of select="format-number( sum($items/data[@name=current()/@name]), $format )"/>
                  </xsl:when>
                  <xsl:when test="@summary='average'">
                    <xsl:value-of select="format-number( sum($items/data[@name=current()/@name]) div count($items/data[@name=current()/@name]), $format )"/>
                  </xsl:when>
                  </xsl:choose>
                </xsl:variable>

                <xsl:call-template name="note">
                  <xsl:with-param name="anchor">
                    <font>
                    <xsl:attribute name="class"><xsl:value-of select="@summary"/></xsl:attribute>
                      <xsl:value-of select="$summary"/>
                    </font>
                  </xsl:with-param>
                  <xsl:with-param name="text">
                    <font>
                    <xsl:attribute name="class"><xsl:value-of select="@summary"/></xsl:attribute>
                      <xsl:call-template name="label">
                        <xsl:with-param name="label" select="@summary"/>
                      </xsl:call-template>
                    </font>
                    <xsl:if test="@count">
                      <br/>
                      <div nowrap="true">
                        <font class="bold">
                          <xsl:call-template name="label">
                            <xsl:with-param name="label" select="'recordCount'"/>
                          </xsl:call-template>
                        </font>
                        <xsl:value-of select="@count"/>
                      </div>
                    </xsl:if>
                  </xsl:with-param>
                  <xsl:with-param name="width" select="50"/>
                </xsl:call-template>
              </xsl:if>

            </td>
          </xsl:for-each>
        </tr>
      </tfoot>
    </xsl:if>
 </table>

</xsl:template>


<!-- ********** TABLE TEMPLATE (INFO STYLE) ********** -->

<xsl:template name="infoTable">
<xsl:param name="Level"/>
<xsl:param name="tableId"/>

  <table class='block'>
  <xsl:attribute name='id'><xsl:value-of select="$tableId"/></xsl:attribute>

    <xsl:for-each select="item[(@level &lt;= $Level or not(@level))][data[not(@visible='false') and (@level &lt;= $Level or not(@level))]]">
      <xsl:for-each select="data[not(@visible='false') and (@level &lt;= $Level or not(@level))]">
        <tr>
          <td width='12%' class='h4' style='white-space:nowrap;'>
            <xsl:if test='not(@name=preceding-sibling::data/@name)'>
              <xsl:call-template name="label">
                <xsl:with-param name="label" select="@name"/>
              </xsl:call-template>:
            </xsl:if>
          </td>
          <td class='info'>
          <xsl:if test='position() mod 2=1'>
            <xsl:attribute name='class'>info b3</xsl:attribute>
          </xsl:if>
            <xsl:call-template name="data"/>

            <xsl:apply-templates select="@warning" mode="anchor"/>
            <xsl:apply-templates select="@note"/>

          </td>
        </tr>
      </xsl:for-each>
      <xsl:if test="position() != last()">
        <tr><td colspan='2'><hr class='thin'/></td></tr>
      </xsl:if>
    </xsl:for-each>
  </table>
</xsl:template>


<!-- ********** ITEM/DATA TEMPLATE ********** -->

<xsl:template match="data">
<xsl:param name="header"/>

  <xsl:variable name="class">
    <xsl:choose>
      <xsl:when test="@class">
        <xsl:value-of select="@class"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="$header/data[@name=current()/@name]/@class"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <td>
  <xsl:if test="position() != 1 and contains( $class, 'number')">
    <xsl:attribute name="width">8%</xsl:attribute>
  </xsl:if>

  <xsl:attribute name="colspan"><xsl:value-of select="@span"/></xsl:attribute>
  <xsl:attribute name="class"><xsl:value-of select="$class"/></xsl:attribute>

    <xsl:value-of select="substring($tab, string-length($tab) - (@tab * 4) )"/>

    <xsl:choose>
      <xsl:when test="@calculate or $header/data[@name=current()/@name]/@calculate">
        <xsl:call-template name="calculate-data">
          <xsl:with-param name="header" select="$header"/>
        </xsl:call-template>
      </xsl:when>
      <xsl:otherwise>
        <xsl:call-template name="data"/>
      </xsl:otherwise>
    </xsl:choose>
    <xsl:apply-templates select="@warning" mode="anchor"/>
    <xsl:apply-templates select="@note"/>

  </td>
</xsl:template>

<!-- ********** CALCUTATED DATA TEMPLATE ********** -->

<xsl:template name="calculate-data">
<xsl:param name="header"/>

  <xsl:variable name="field">
    <xsl:choose>
      <xsl:when test="$header/data[@name=current()/@name]/@field">
        <xsl:value-of select="$header/data[@name=current()/@name]/@field"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="@field"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="calculate">
    <xsl:choose>
      <xsl:when test="$header/data[@name=current()/@name]/@calculate">
        <xsl:value-of select="$header/data[@name=current()/@name]/@calculate"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="@calculate"/>
      </xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="format">
    <xsl:choose>
      <xsl:when test="$header/data[@name=current()/@name]/@format">
        <xsl:value-of select="$header/data[@name=current()/@name]/@format"/>
      </xsl:when>
      <xsl:when test="@format"><xsl:value-of select="@format"/></xsl:when>
      <xsl:otherwise>0.00</xsl:otherwise>
    </xsl:choose>
  </xsl:variable>

  <xsl:variable name="sum">
    <xsl:value-of select="sum(parent::item/item/data[@name=$field])"/>
  </xsl:variable>

  <xsl:choose>
    <xsl:when test="$calculate='total'">
      <xsl:value-of select="format-number($sum,$format)"/>
    </xsl:when>
    <xsl:when test="$calculate='average'">
      <xsl:variable name="count">
        <xsl:value-of select="count(parent::item/item/data[@name=$field])"/>
      </xsl:variable>
      <xsl:value-of select="format-number($sum div $count,$format)"/>
    </xsl:when>
  </xsl:choose>

</xsl:template>

<!-- ********** COLUMN HEADER TEMPLATE ********** -->

<xsl:template match="data" mode="header">
<xsl:param name="sort"/>
<xsl:param name="tableId"/>
  <th class='header'>
  <xsl:attribute name='colspan'><xsl:value-of select="@span"/></xsl:attribute>
  <xsl:attribute name="class">
    <xsl:choose>
      <xsl:when test="not(@class)"></xsl:when>
      <xsl:when test="@class='code' or @class='icon' or @class='string'">
        string
      </xsl:when>
      <xsl:when test="@class='span'">
        span
      </xsl:when>
      <xsl:otherwise>
        number
      </xsl:otherwise>
    </xsl:choose>
  </xsl:attribute>
  <xsl:if test="$sort = 'true'">
    <xsl:attribute name="onclick">sort(<xsl:value-of select="$tableId"/>);</xsl:attribute>
    <xsl:attribute name="style">cursor:hand;</xsl:attribute>
  </xsl:if>

  <xsl:call-template name='title'/>

  <xsl:apply-templates select="@warning"/>
  <xsl:apply-templates select="@note"/>

  </th>
</xsl:template>

<!-- ********** DATA TEMPLATE ********** -->

<xsl:template name="data">
<xsl:param name="format"/>

  <xsl:choose>
    <xsl:when test="@translate = 'value'">
      <xsl:call-template name="label">
        <xsl:with-param name='label' select='.'/>
      </xsl:call-template>
    </xsl:when>
    <xsl:when test="string-length($format)">
      <xsl:value-of select='format-number( ., $format )'/>
    </xsl:when>
    <xsl:when test="@format">
      <xsl:value-of select='format-number( ., @format )'/>
    </xsl:when>
    <xsl:when test="ancestor::table/header/data[@name=current()/@name]/@format">
      <xsl:value-of select='format-number( ., ancestor::table/header/data[@name=current()/@name]/@format )'/>
    </xsl:when>
    <xsl:otherwise><xsl:value-of select='.'/></xsl:otherwise>
  </xsl:choose>
  <xsl:if test="@units">
    <xsl:value-of select="'&#xA0;'"/><xsl:value-of select="@units"/>
  </xsl:if>

</xsl:template>

<!-- ********** LABEL TEMPLATE ********** -->

<xsl:template name="label">
<xsl:param name="label"/>

  <xsl:choose>
    <xsl:when test="@translate='false'">
      <xsl:value-of select="$label"/>
    </xsl:when>
    <xsl:otherwise>
      <xsl:variable name='title'>
        <xsl:value-of select="msxsl:node-set($titles)/title[@name = $label]"/>
      </xsl:variable>
      <xsl:choose>
        <xsl:when test='string-length($title)'><xsl:value-of select="$title"/></xsl:when>
        <xsl:otherwise><xsl:value-of select="$label"/></xsl:otherwise>
      </xsl:choose>
    </xsl:otherwise>
  </xsl:choose>

</xsl:template>

<!-- ********** TITLE TEMPLATE ********** -->

<xsl:template name='title'>
<xsl:param name='nosub'/>

    <xsl:variable name="label">
      <xsl:choose>
        <xsl:when test="@title"><xsl:value-of select="@title"/></xsl:when>
        <xsl:otherwise><xsl:value-of select="@name"/></xsl:otherwise>
      </xsl:choose>
    </xsl:variable>

    <xsl:call-template name="label">
      <xsl:with-param name="label" select="$label"/>
    </xsl:call-template>

    <xsl:if test="@index">
      <xsl:value-of select="'&#xA0;'"/>
      <xsl:value-of select="@index"/>
    </xsl:if>

    <xsl:if test="@subtitle and $nosub!='1'">
      <div class="h3">
        <xsl:call-template name="label">
          <xsl:with-param name="label" select="@subtitle"/>
        </xsl:call-template>
      </div>
    </xsl:if>

</xsl:template>

<!-- ********** WARNING POPUP TEMPLATE ********** -->

<xsl:template match="@warning" mode="anchor">
  <a><xsl:attribute name="name"><xsl:value-of select="."/></xsl:attribute></a>
  <xsl:apply-templates select="."/>
</xsl:template>

<xsl:template match="@warning">
  <xsl:variable name='id'>w_<xsl:value-of select="generate-id(.)"/><xsl:value-of select="ms:tag()"/>_<xsl:value-of select="@name"/></xsl:variable>
  <a style="cursor:help">
  <xsl:attribute name="onMouseOver">popup(<xsl:value-of select='$id'/>)</xsl:attribute>
  <xsl:attribute name="onMouseOut"><xsl:value-of select='$id'/>.style.display='none'</xsl:attribute>
    <font style="font-weight:bolder;" color="red" face="wingdings" size="x-small"><xsl:value-of select="'&#x4F;'"/></font>
  </a>
  <div class="popup" style="display:'none';width:300;">
  <xsl:attribute name="id"><xsl:value-of select='$id'/></xsl:attribute>
    <xsl:choose>
      <xsl:when test="/report/warnings/warning[@name=current()]">
        <xsl:copy-of select="/report/warnings/warning[@name=current()]"/>
      </xsl:when>
      <xsl:otherwise>
        <xsl:value-of select="."/>
      </xsl:otherwise>
    </xsl:choose>
  </div>
</xsl:template>

<!-- ********** NOTE POPUP TEMPLATE ********** -->

<xsl:template match="@note">
  <xsl:call-template name="note">
    <xsl:with-param name="text">
      <xsl:element name="note"><xsl:value-of select="."/></xsl:element>
    </xsl:with-param>
    <xsl:with-param name="width" select="'300'"/>
  </xsl:call-template>
</xsl:template>

<xsl:template name="note">
<xsl:param name="text"/>
<xsl:param name="width"/>
<xsl:param name="anchor"/>

  <xsl:variable name='id'>popup_<xsl:value-of select="ms:tag()"/></xsl:variable>
  <a style="cursor:help">
  <xsl:attribute name="onMouseOver">popup(<xsl:value-of select='$id'/>)</xsl:attribute>
  <xsl:attribute name="onMouseOut"><xsl:value-of select='$id'/>.style.display='none'</xsl:attribute>
    <xsl:choose>
    <xsl:when test="$anchor">
      <xsl:copy-of select="$anchor"/>
    </xsl:when>
    <xsl:otherwise>
      <div style="margin-left:3;position:absolute;width:8;height:10;background:infobackground;">
        <div style="position:absolute;top:-5;left:-3;">
          <font style="font-size: 11pt;font-weight:normal;" color="black" face="webdings"><xsl:value-of select="'&#x9D;'"/></font>
        </div>
      </div>
    </xsl:otherwise>
    </xsl:choose>
  </a>
  <div class="popup">
  <xsl:attribute name="style">display:'none';width:<xsl:value-of select="$width"/></xsl:attribute>
  <xsl:attribute name="id"><xsl:value-of select='$id'/></xsl:attribute>
    <xsl:copy-of select="$text"/>
  </div>
</xsl:template>

<!-- ********** WARNINGS TABLE TEMPLATE ********** -->

<xsl:template name="warning">

<xsl:if test="//@warning">
<a name="warnings"/>
<table class="block">
    <tr>
      <td>
        <table width='100%'>
          <tr>
            <td class='h2'>
              <xsl:call-template name="label">
                <xsl:with-param name="label" select="'warnings'"/>
              </xsl:call-template>
            </td>
            <td align='right'>
              <a href="#top">
                <font style='text-decoration:none;' face="webdings" size="x-small">
                  <xsl:value-of select="'&#x35;'"/>
                </font>
              </a>
            </td>
          </tr>
        </table>
    <hr/></td></tr>
    <tr><td>
      <table width='100%'>
      <tr>
      <xsl:if test="/report/warnings">
        <th class='string'>
          <xsl:call-template name="label">
            <xsl:with-param name="label" select="'type'"/>
          </xsl:call-template>
        </th>
      </xsl:if>
      <th colspan='2' class='string'>
        <xsl:call-template name="label">
          <xsl:with-param name="label" select="'item'"/>
        </xsl:call-template>
      </th>
      <th class='string'>
        <xsl:call-template name="label">
          <xsl:with-param name="label" select="'warning'"/>
        </xsl:call-template>
      </th>
      <xsl:if test="/report/warnings">
        <th class='string'>
          <xsl:call-template name="label">
            <xsl:with-param name="label" select="'help'"/>
          </xsl:call-template>
        </th>
      </xsl:if>
      </tr>
      <xsl:for-each select='//@warning[parent::data]'>
        <xsl:variable name="warning" select="."/>
        <tr>
        <xsl:if test="position() mod 2 = 1"><xsl:attribute name='class'>b3</xsl:attribute></xsl:if>
          <xsl:if test="/report/warnings">
            <td valign='top' align='left'  style='white-space:nowrap'>
              <xsl:choose>
                <xsl:when test="/report/warnings/warning[@name=current()]/@type='info'">
                  <table><tr><td width="26px" align="center"><xsl:call-template name="infoIcon"/></td><td class="info">Information</td></tr></table>
                </xsl:when>
                <xsl:when test="/report/warnings/warning[@name=current()]/@type='warning' or not(/report/warnings/warning[@name=current()]/@type)">
                  <table><tr><td width="26px" align="center"><xsl:call-template name="warningIcon"/></td><td class="info">Warning</td></tr></table>
                </xsl:when>
                <xsl:when test="/report/warnings/warning[@name=current()]/@type='error'">
                  <table><tr><td width="26px" align="center"><xsl:call-template name="errorIcon"/></td><td class="info">Error</td></tr></table>
                </xsl:when>
              </xsl:choose>
            </td>
          </xsl:if>
          <td valign="top" style="white-space:nowrap;padding:6px;" class="h4">
            <xsl:for-each select="parent::data">
              <a>
              <xsl:attribute name='href'>#<xsl:value-of select="$warning"/></xsl:attribute>
                <xsl:call-template name='title'/>
              </a>
            </xsl:for-each>
          </td>
          <td valign="top" class="info" style="padding:6px;">
            <xsl:for-each select="parent::data">
              <xsl:call-template name="data"/>
            </xsl:for-each>
          </td>
          <td>
            <xsl:choose>
              <xsl:when test="/report/warnings/warning[@name=current()]">
                <xsl:copy-of select="/report/warnings/warning[@name=current()]"/>
              </xsl:when>
              <xsl:otherwise>
                <xsl:value-of select="."/>
              </xsl:otherwise>
            </xsl:choose>
          </td>
          <xsl:if test="/report/warnings">
            <td valign='top' align='center' style="padding:2px;">
              <xsl:if test="string-length(/report/warnings/warning[@name=current()]/@url)">
                <a>
                <xsl:attribute name="onclick">help('<xsl:value-of select="/report/warnings/warning[@name=current()]/@url"/>')</xsl:attribute>
                  <xsl:call-template name="helpIcon"/>
                </a>
              </xsl:if>
            </td>
          </xsl:if>
        </tr>
      </xsl:for-each>
      </table>
    </td></tr>
</table>
<br/>
</xsl:if>
</xsl:template>

<xsl:template name="infoIcon">
  <div style="position:relative;text-overflow:clip;overflow:hidden;width:22px;height:22px;">
    <font style="font-size:18pt; text-decoration:none;font-weight:normal;color:gray" face="webdings"><xsl:value-of select="'&#40;'"/></font>
    <div style="position:absolute;top:-1;left:-1;">
      <font style="font-size:18pt;text-decoration:none;font-weight:normal;color:aliceblue;" face="webdings"><xsl:value-of select="'&#40;'"/></font>
    </div>
    <div style="position:absolute;top:0;left:7;">
      <font style="font-size:8pt;text-decoration:none;font-weight:bolder;color:blue;" face="courier">i</font>
    </div>
  </div>
</xsl:template>

<xsl:template name="warningIcon">
  <div style="position:relative;text-overflow:clip;overflow:hidden;width:22px;height:22px;">
    <font style="font-size:19pt;text-decoration:none;font-weight:normal;color:gray" face="webdings"><xsl:value-of select="'&#234;'"/></font>
    <div style="position:absolute;top:3;left:11;">
      <font style="font-size:8pt;text-decoration:none;font-weight:bolder;color:grey">!</font>
    </div>
    <div style="position:absolute;top:-1;left:-1;">
      <font style="font-size:19pt;text-decoration:none;font-weight:normal;color:yellow;" face="webdings"><xsl:value-of select="'&#234;'"/></font>
    </div>
    <div style="position:absolute;top:2;left:10;">
      <font style="font-size:8pt;text-decoration:none;font-weight:bolder;color:yellow;">!</font>
    </div>
  </div>
</xsl:template>

<xsl:template name="errorIcon">
  <div style="position:relative;">
    <font style="font-size:16pt;text-decoration:none;font-weight:normal;color:gray" face="webdings"><xsl:value-of select="'&#110;'"/></font>
    <div style="position:absolute;top:-1;left:-1;">
      <font style="font-size:16pt;text-decoration:none;font-weight:normal;color:red" face="webdings"><xsl:value-of select="'&#110;'"/></font>
    </div>
    <div style="position:absolute;top:-1;left:-1;">
      <font style="font-size:16pt;text-decoration:none;font-weight:normal;color:white" face="webdings"><xsl:value-of select="'&#114;'"/></font>
    </div>
  </div>
</xsl:template>

<xsl:template name="helpIcon">
  <div style="position:relative;">
    <font style="font-size:18pt;text-decoration:none;font-weight:normal;" face="webdings"><xsl:value-of select="'&#157;'"/></font>
    <div style="position:absolute;top:-2;left:7;">
      <font style="font-size:12pt;text-decoration:none;font-weight:bolder;color:yellow;" face="webdings"><xsl:value-of select="'&#115;'"/></font>
    </div>
  </div>
</xsl:template>

<!-- ********** +/- ICON TEMPLATE ********** -->

<xsl:template name="expand">
<xsl:param name="id"/>
<xsl:param name="state"/>
  <div style="border: solid black 1px; position: relative; width: 11px; height: 11px;">
  <div style="font-size: 8pt;position: absolute; overflow: hidden; width: 8; height: 9; line-height: 9px; top: -1; left: 0;" >
  <xsl:attribute name="id"><xsl:value-of select="$id"/></xsl:attribute>
    <xsl:value-of select="$state"/>
  </div>
  <div style="position: absolute; overflow: hidden;width: 7; height: 9; line-height: 9px; left: 1; top: -3;" >
    <hr style="height:1px"/>
  </div>
  </div>
</xsl:template>


</xsl:stylesheet>
