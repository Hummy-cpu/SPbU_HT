namespace PhoneBook.Core
open System
type Contact = {
    Name: string
    Phone: string
}
type PhoneBookError =
    | InvalidName of string      
    | InvalidPhone of string
    | ContactAlreadyExists       // без данных
    | FileError of string 
    
module PhoneBook =
    let validateName (name: string) =
        if String.IsNullOrWhiteSpace(name) then
            Error (InvalidName "Имя не может быть пустым")
        elif name.Length > 50 then
            Error (InvalidName "Имя слишком длинное")
        else
            Ok (name.Trim())

    let validatePhone (phone: string) =
            if String.IsNullOrWhiteSpace(phone) then
                Error (InvalidPhone "Телефон не может быть пустым")
            else
                let cleaned = phone.Trim()
                if cleaned |> Seq.forall Char.IsDigit then
                    Ok cleaned
                else
                    Error (InvalidPhone "Телефон должен содержать только цифры")
    let addContact (contacts: Contact list) (name: string) (phone: string) =
        match (validateName name), (validatePhone phone) with
        | Ok validName, Ok validPhone ->
            let exists = contacts |> List.exists (fun c -> c.Name = validName)
            if exists then
                Error ContactAlreadyExists 
            else
                let newContact = { Name = validName; Phone = validPhone }
                Ok (newContact :: contacts)
        | Error e, _ -> Error e
        | _, Error e -> Error e                
    let findByName (contacts: Contact list) (name: string) =
        match validateName name with
        | Ok validName ->
            match contacts |> List.tryFind (fun c -> c.Name = validName) with
            | Some contact -> Ok contact.Phone
            | None -> Error (InvalidName $"Контакт с именем '{validName}' не найден!")
        | Error e -> Error e
    let findByPhone (contacts: Contact list) (phone: string) =
        match validatePhone phone with
        | Ok validPhone ->
            match contacts |> List.tryFind (fun c -> c.Phone = validPhone) with
            | Some contact -> Ok contact.Name
            | None -> Error (InvalidPhone $"Контакт с телефоном '{validPhone}' не найден")
        | Error e -> Error e    
    let getAllContactsSimple (contacts: Contact list) =
        if List.isEmpty contacts then
            []
        else
            contacts
            |> List.sortBy (fun c -> c.Name)
            |> List.map (fun c -> sprintf "Имя: %s, Телефон: %s" c.Name c.Phone)    
    open System.IO

    let saveToFile (contacts: Contact list) (filePath: string) =
        try
            let lines = 
                contacts
                |> List.map (fun c -> $"{c.Name}|{c.Phone}")
            File.WriteAllLines(filePath, lines)
            Ok ()
        with
        | :? PathTooLongException as ex -> Error (FileError $"Путь к файлу слишком длинный: {ex.Message}")
        | :? UnauthorizedAccessException as ex -> Error (FileError $"Нет доступа к файлу: {ex.Message}")
        | :? ArgumentException as ex -> Error (FileError $"Некорректный путь к файлу: {ex.Message}")
        | :? IOException as ex -> Error (FileError $"Ошибка записи файла: {ex.Message}")
    let loadFromFile (filePath: string) =
        try
            if not (File.Exists(filePath)) then
                Error (FileError $"Файл '{filePath}' не найден")
            else
                let lines = File.ReadAllLines(filePath)
                let contacts = 
                    lines
                    |> Array.toList
                    |> List.choose (fun line ->
                        match line.Split('|') with
                        | [| name; phone |] ->
                            match (validateName name), (validatePhone phone) with
                            | Ok validName, Ok validPhone ->
                                Some { Name = validName; Phone = validPhone }
                            | _ -> None
                        | _ -> None
                    )
                Ok contacts
        with
        | :? FileNotFoundException as ex -> 
            Error (FileError $"Файл не найден: {ex.Message}")
        | :? DirectoryNotFoundException as ex -> 
            Error (FileError $"Директория не найдена: {ex.Message}")
        | :? UnauthorizedAccessException as ex -> 
            Error (FileError $"Нет доступа к файлу: {ex.Message}")
        | :? IOException as ex -> 
            Error (FileError $"Ошибка чтения файла: {ex.Message}")
        | :? ArgumentException as ex -> 
            Error (FileError $"Некорректный путь к файлу: {ex.Message}")