(*
    Reading out bindings
*)

type Direction =
    | Trigger
    | In
    | Out

type Properties = Map<string,string>

type Binding = {
    Argument:string
    Direction:Direction
    Type:string
    Properties:Properties
    }
    with member this.Value key = this.Properties.TryFind key

open FSharp.Data
open FSharp.Data.JsonExtensions

let bindingType (``type``:string, dir:string) =
    if ``type``.EndsWith "Trigger"
    then 
        Trigger, ``type``.Replace("Trigger","")
    else 
        if (dir = "in") then In, ``type``
        elif (dir = "out") then Out, ``type``
        else failwith "Unknown binding"

let extractBindings (contents:string) =
    contents
    |> JsonValue.Parse
    |> fun elements -> elements.GetProperty "bindings"
    |> JsonExtensions.AsArray
    |> Array.map (fun binding -> 
        // retrieve the properties we care about
        let ``type`` = binding?``type``.AsString()
        let direction = if ``type``.EndsWith "Trigger" then "in" else binding?direction.AsString()
        let name = binding?name.AsString()
        // retrieve the "other" properties
        let properties = 
            binding.Properties
            |> Array.filter (fun (key,value) -> 
                key <> "type" && key <> "name" && key <> "direction" && key <> "methods")
            |> Array.map (fun (key,value) -> key, value.AsString())
            |> Map
        // detect the direction and type
        let direction, ``type`` = bindingType (``type``,direction)
        // create and return a binding
        {
            Type = ``type``
            Direction = direction
            Argument = name
            Properties = properties
        }
        )

(*
    Reading out packages
*)

type Package = {
    Name:string
    Version:string
    }

(*
Extracting all the data from a root folder
*)

open System.IO

let candidates root = 
    root
    |> Directory.EnumerateDirectories
    |> Seq.filter (fun dir ->
        Directory.EnumerateFiles(dir)
        |> Seq.map FileInfo
        |> Seq.exists (fun file -> file.Name = "function.json")
        )
    |> Seq.map DirectoryInfo

type AppGraph = {
    Bindings: (string * Binding) []
    }

let extractGraph (root:string) =

    let functions = candidates root

    let bindings = 
        functions
        |> Seq.map (fun dir ->
            let functionName = dir.Name
            Path.Combine (dir.FullName,"function.json")
            |> File.ReadAllText
            |> extractBindings
            |> Array.map (fun binding -> 
                functionName, binding)
            )
        |> Seq.collect id
        |> Seq.toArray
    
    {
        Bindings = bindings
    }
    
(*
Rendering the graph
*)

let quoted (text:string) = sprintf "\"%s\"" text
let indent = "    "

let bindingDescription (functionName: string, binding:Binding) =
    match binding.Type with
    | "timer" -> "Timer"
    | "queue" -> "Queue " + (binding.Properties.["queueName"])
    | "blob" -> "Blob " + (binding.Properties.["path"])
    | "http" -> "HTTP /" + functionName
    | _ -> binding.Type
    |> quoted

let packageDescription (package:Package) =
    sprintf "%s (%s)" package.Name package.Version
    |> quoted

let functionDescription = quoted

let renderFunctionNodes format (graph:AppGraph) =
    let functionNames = 
        graph.Bindings
        |> Seq.map (fst >> functionDescription)
        |> Seq.distinct
    Seq.append
        [ format ]
        functionNames 
        |> Seq.map (fun name -> indent + name) 
    |> String.concat "\n"

let renderBindingNodes format (graph:AppGraph) =
    let bindingNames = 
        graph.Bindings
        |> Seq.map bindingDescription
        |> Seq.distinct
    Seq.append
        [ format ]
        bindingNames 
        |> Seq.map (fun name -> indent + name) 
    |> String.concat "\n"

let renderTriggers format (graph:AppGraph) =
    let triggers =
        graph.Bindings
        |> Seq.filter (fun (_,binding) -> binding.Direction = Trigger)
        |> Seq.map (fun (fn,binding) -> 
            sprintf "%s -> %s [ label = %s ]" 
                (bindingDescription (fn, binding))
                (functionDescription fn)
                (binding.Argument |> quoted)
            )
        |> Seq.distinct

    Seq.append
        [ format ]
        triggers 
        |> Seq.map (fun name -> indent + name) 
    |> String.concat "\n"

let renderInBindings format (graph:AppGraph) =
    let bindings =
        graph.Bindings
        |> Seq.filter (fun (_,binding) -> binding.Direction = In)
        |> Seq.map (fun (fn,binding) -> 
            sprintf "%s -> %s [ label = %s ]" 
                (bindingDescription (fn, binding))
                (functionDescription fn)
                (binding.Argument |> quoted)
            )
        |> Seq.distinct
  
    Seq.append
        [ format ]
        bindings 
        |> Seq.map (fun name -> indent + name) 
    |> String.concat "\n"

let renderOutBindings format (graph:AppGraph) =
    let bindings =
        graph.Bindings
        |> Seq.filter (fun (_,binding) -> binding.Direction = Out)
        |> Seq.map (fun (fn,binding) -> 
            sprintf "%s -> %s [ label = %s ]" 
                (functionDescription fn)
                (bindingDescription (fn, binding))               
                (binding.Argument |> quoted)
            )
        |> Seq.distinct

    Seq.append
        [ format ]
        bindings 
        |> Seq.map (fun name -> indent + name) 
    |> String.concat "\n"

type GraphFormat = {
    FunctionNode:string
    BindingNode:string
    PackageNode:string
    Trigger:string
    InBinding:string
    OutBinding:string
}

let graphFormat = {
    FunctionNode = "node [shape=doublecircle,style=filled,color=orange]"
    BindingNode = "node [shape=box,style=filled,color=yellow]"
    PackageNode = "node [shape=box,style=filled,color=lightblue]"
    Trigger = "edge [ style=bold ]"
    InBinding = "edge [ style=solid ]"
    OutBinding = "edge [ style=solid ]"
    }

let renderGraph (format:GraphFormat) (app:AppGraph) =
    let functionNodes = renderFunctionNodes format.FunctionNode app
    let bindingrNodes = renderBindingNodes format.BindingNode app
    let triggers = renderTriggers format.Trigger app
    let ins = renderInBindings format.InBinding app
    let outs = renderOutBindings format.OutBinding app

    sprintf """digraph app {
%s
%s
%s    
%s    
%s    
}""" functionNodes bindingrNodes triggers ins outs


[<EntryPoint>]
let main argv = 
    // location on disk
    let root = argv.[0]// @"C:\Work\F#unctions\Demos\9-app-m"

    // generate a graphviz file
    root
    |> extractGraph 
    |> renderGraph graphFormat 
    |> fun content -> 
        File.WriteAllText(argv.[1], content)

    0 // return an integer exit code
