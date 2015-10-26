namespace <%= baseName %>.F

module Domain = 

  open System
  open System.Net
  open System.IO
  open System.Text
  open System.Data
  open System.Data.SQLite

  open Suave
  open Suave.Http
  open Suave.Http.Applicatives
  open Suave.Http.Files
  open Suave.Http.Successful
  open Suave.Web
  open Suave.Logging
  open Suave.Types

  open Dapper

  open Newtonsoft.Json
  open Newtonsoft.Json.Converters


  <% _.each(entities, function (entity) { %>
  [<CLIMutable>]
  [<JsonObject(MemberSerialization.OptIn)>]
  type <%= _.capitalize(entity.name) %> = {
    
    [<JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)>]
    mutable Id : int
    <% _.each(entity.attrs, function (attr) { %>
    [<JsonProperty("<%= attr.attrName %>")>]
    mutable <%= _.capitalize(attr.attrName) %> : <%= attr.attrImplType %><% }); %>
  }<% }); 

  %>