module MonkeyInterpreter.Eval.Object

open System
open MonkeyInterpreter


type Environment =
    { Map: Map<string, Object>
      Outer: Environment Option } 
with
    static member Empty =
        { Map = [ ] |> Map.ofList
          Outer = None }
    
    static member CreateEnclosedEnv outerEnv =
        { Map = [ ] |> Map.ofList
          Outer = Some outerEnv }
    
    member this.Set name object = { this with Map = this.Map.Add (name, object) }
    
    member this.Get name =
        match Map.tryFind name this.Map with
        | Some value ->
            Some value
        | None when this.Outer.IsSome ->
            let outerEnv = (Option.get this.Outer)
            outerEnv.Get name
        | _ ->
            None
          
          

and Object =
    | IntegerType of Int64
    | BooleanType of bool
    | NullType
    | StringType of string
    | FunctionType of Function
    | ErrorType of ErrorType
    | ArrayType of Object list 
with
    member this.Type() =
        match this with
        | IntegerType _ -> "INTEGER"
        | BooleanType _ -> "BOOLEAN"
        | NullType -> "NULL"
        | StringType _ -> "STRING"
        | FunctionType _ -> "FUNCTION" 
        | ErrorType _ -> "ERROR" 
        | ArrayType _ -> "ARRAY" 
        
    member this.Inspect() = this.ToString()
        
    override this.ToString() =
        match this with
        | IntegerType integer -> $"{integer}"
        | BooleanType boolean -> $"{boolean}"
        | NullType -> "null"
        | StringType string -> string
        | FunctionType _ -> failwith "todo" 
        | ErrorType _ -> failwith "todo" 
        | ArrayType arr ->
            let elementsString = String.concat ", " (arr |> List.map (_.ToString()))
            $"[{elementsString}]"



and Function =
    | UserFunction of UserFunction
    | BuiltinFunction of BuiltinFunction
    
    
and UserFunction =
    { Parameters: Identifier list
      Body: BlockStatement
      Env: Environment }
with
    static member FromFunctionLiteral environment (functionLiteral: FunctionLiteral) =
        { Parameters = functionLiteral.Parameters; Body = functionLiteral.Body; Env = environment }
        
    override this.ToString() =
        let commaSeparatedParameters = String.concat ", " (this.Parameters |> List.map (_.Value)) 
        $"fn ({commaSeparatedParameters}) {{ {this.Body.ToString()} }}" 

and BuiltinFunction =
    { Fn: Object list -> Object
      ParametersLength: int }


and ErrorType = ErrorType of string
with
    member this.GetMsg =
        let (ErrorType e) = this
        e