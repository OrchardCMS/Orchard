/// <reference path="jquery/jquery.d.ts"/>

interface ExpandoOptions {
    collapse: boolean;
    remember: boolean;
}

interface JQuery {
    expandoControl(): JQuery;
    expandoControl(controller: Function, options: ExpandoOptions): JQuery;
}