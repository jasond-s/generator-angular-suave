namespace <%= baseName %>.F

module App = 

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

  open <%= baseName %>.F.Domain

  
  let logger = Loggers.saneDefaultsFor LogLevel.Debug

  let dbFactory () =
    let dbConnectionFactory = new SQLiteConnection ("my.db")
    dbConnectionFactory.Open ()
    dbConnectionFactory :> IDbConnection


  type CustomDateTimeConverter() =
    inherit IsoDateTimeConverter()

    do base.DateTimeFormat <- "yyyy-MM-dd"

  let converters : JsonConverter[] = [| CustomDateTimeConverter() |]

  
  // JSON helpers

  let toBytes (s: string) = s |> Encoding.UTF8.GetBytes

  let toJson<'a> (o : 'a) = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(o, converters))

  let fromJson<'a> (bytes : byte []) = JsonConvert.DeserializeObject<'a>(Encoding.UTF8.GetString(bytes), converters)

  let mapJson f (r : HttpRequest) = f (fromJson (r.rawForm)) |> toJson |> Successful.ok

  let localIpAddress : IPAddress = IPAddress.Loopback

  let localPort : Sockets.Port = Sockets.Port.Parse "8080"

  let mimeTypes = 
      Writers.defaultMimeTypesMap
      >=> (function 
          | ".woff" -> Writers.mkMimeType "application/font-woff" false
          | ".woff2"-> Writers.mkMimeType "application/font-woff2" false
          | ".ttf"  -> Writers.mkMimeType "application/font-ttf" false
          | ".eot"  -> Writers.mkMimeType "application/vnd.ms-fontobject" false
          | ".otf"  -> Writers.mkMimeType "application/font-otf" false
          | ".svg"  -> Writers.mkMimeType "image/svg+xml" false
          | _ -> None)



  <% _.each(entities, function (entity) { %>
  let <%= entity.name %>Part : WebPart =
    choose [
      GET >>= path "/<%= baseName %>/<%= pluralize(entity.name) %>" >>= request(
        (fun _ -> 
          use db = dbFactory ()
          let rows = db.Query<<%= _.capitalize(entity.name) %>>("select * from <%= _.capitalize(entity.name) %>")
          toJson rows |> Successful.ok));

      GET >>= pathScan "/<%= baseName %>/<%= pluralize(entity.name) %>/%d"
        (fun id -> 
          use db = dbFactory ()
          let row = db.Query<<%= _.capitalize(entity.name) %>>("select * from <%= _.capitalize(entity.name) %> where id == @id", new { id = id })
          toJson row |> Successful.ok);

      POST >>= path "/<%= baseName %>/<%= pluralize(entity.name) %>" >>= request(map_json 
        (fun (row : <%= _.capitalize(entity.name) %>) -> 
          use db = dbFactory ()
          let num = db.Query<int>("insert into <%= _.capitalize(entity.name) %> values (%value); last_insert_rowid()", new { value = <%= _.capitalize(entity.name) %>}) |> Seq.head
          row.Id <- num
          row));

      PUT >>= pathScan "/<%= baseName %>/<%= pluralize(entity.name) %>/%d"
        (fun id -> request(map_json (fun (row : <%= _.capitalize(entity.name) %>) -> 
          row.Id <- id
          use db = dbFactory ()
          let old = db.Query<<%= _.capitalize(entity.name) %>>("select * from <%= _.capitalize(entity.name) %> where id == @id", new { id = id }) |> Seq.head
          let num = db.Query("update <%= _.capitalize(entity.name) %> set @value where id = @id", new { value = <%= _.capitalize(entity.name) %>, id = <%= _.capitalize(entity.name) %>.id})
          row)));

      DELETE >>= pathScan "/<%= baseName %>/<%= pluralize(entity.name) %>/%d"
        (fun id -> 
          use db = dbFactory ()
          let num = db.Query<<%= _.capitalize(entity.name) %>>("delete from <%= _.capitalize(entity.name) %> where id = @id", new { id = id })
          Successful.no_content);
    ]<% }); %>          



  // WEB SERVER

  let config = { 
      bindings              = [ HttpBinding.mk HTTP localIpAddress localPort ]
      serverKey             = toBytes "vC8hd46jydisFLu3oAhD9MprhMTXQQ2WQsbGA5MW"
      errorHandler          = defaultErrorHandler
      listenTimeout         = TimeSpan.FromMilliseconds 2000.
      cancellationToken     = Async.DefaultCancellationToken
      bufferSize            = 2048
      maxOps                = 100
      mimeTypesMap          = mimeTypes
      homeFolder            = Some(Path.Combine(Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location), "Content"))
      logger                = logger 
      compressedFilesFolder = None
      }
      

  let app = choose [ 
              log logger logFormat >>= never

              // Serve the app.
              GET >>= path "/" >>= browseFileHome "index.html"

              <% _.each(entities, function (entity) { %>
              <%= entity.name %>Part; <% }); %>                

              //serves file if it exists
              GET >>= browseHome 

              // Defaults
              RequestErrors.NOT_FOUND "Found no handlers" 
          ]

