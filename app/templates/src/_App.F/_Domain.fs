namespace <%= baseName %>.F

module Domain = 

  open System
  open System.Net

  open Suave
  open Suave.Web
  open Suave.Http
  open Suave.Http.Applicatives
  open Suave.Http.Files
  open Suave.Http.Successful
  open Suave.Types
  open Suave.Session
  open Suave.Log
  open System.IO
  open System.Text

  open System.Data
  open ServiceStack.DataAnnotations
  open ServiceStack.OrmLite
  open ServiceStack.OrmLite.Sqlite

  open Newtonsoft.Json
  open Newtonsoft.Json.Converters


  <% _.each(entities, function (entity) { %>
  [<CLIMutable>]
  [<JsonObject(MemberSerialization.OptIn)>]
  type <%= _.capitalize(entity.name) %> = {
    [<AutoIncrement>]
    [<JsonProperty(PropertyName = "id", NullValueHandling = NullValueHandling.Ignore)>]
    mutable Id : int
    <% _.each(entity.attrs, function (attr) { %>
    [<JsonProperty("<%= attr.attrName %>")>]
    mutable <%= _.capitalize(attr.attrName) %> : <%= attr.attrImplType %><% }); %>
  }<% }); 

  %>