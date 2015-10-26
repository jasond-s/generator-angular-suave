open Suave
open Suave.Http
open Suave.Web


// 
// - Open the F# Module
//
open <%= _.capitalize(baseName) %>.F.App


//
// - Start the web server
//
startWebServer config app