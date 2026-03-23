namespace PhoneBook.Console

open System
open PhoneBook.Core

module UserInterface =
    
    let private showMenu () =
        printfn "\n=== ТЕЛЕФОННЫЙ СПРАВОЧНИК ==="
        printfn "1. Добавить контакт"
        printfn "2. Найти телефон по имени"
        printfn "3. Найти имя по телефону"
        printfn "4. Показать все контакты"
        printfn "5. Сохранить в файл"
        printfn "6. Загрузить из файла"
        printfn "0. Выход"
        printfn "---------------------------"
        printf "Выберите действие: "
    
    let private showError error =
        match error with
        | InvalidName msg -> 
            printfn "Ошибка имени: %s" msg
        | InvalidPhone msg -> 
            printfn "Ошибка телефона: %s" msg
        | ContactAlreadyExists -> 
            printfn "Контакт с таким именем уже существует"
        | FileError msg -> 
            printfn "Ошибка файла: %s" msg
    
    let private showContacts (contacts: string list) =
        if List.isEmpty contacts then
            printfn " Телефонная книга пуста"
        else
            printfn "=== ВСЕ КОНТАКТЫ ==="
            contacts |> List.iter (printfn "%s")
    
    let rec runLoop (contacts: Contact list) =
        showMenu()
        
        match Console.ReadLine() with
        | "1" -> // Добавить контакт
            printf "Введите имя: "
            let name = Console.ReadLine()
            printf "Введите телефон: "
            let phone = Console.ReadLine()
            
            match PhoneBook.addContact contacts name phone with
            | Ok newContacts ->
                printfn "Контакт успешно добавлен!"
                runLoop newContacts
            | Error error ->
                showError error
                runLoop contacts
        
        | "2" -> // Найти телефон по имени
            printf "Введите имя: "
            let name = Console.ReadLine()
            
            match PhoneBook.findByName contacts name with
            | Ok phone ->
                printfn "Телефон: %s" phone
            | Error error ->
                showError error
            
            printfn "\nНажмите Enter для продолжения..."
            Console.ReadLine() |> ignore
            runLoop contacts
        
        | "3" -> // Найти имя по телефону
            printf "Введите телефон: "
            let phone = Console.ReadLine()
            
            match PhoneBook.findByPhone contacts phone with
            | Ok name ->
                printfn "Имя: %s" name
            | Error error ->
                showError error
            
            printfn "\nНажмите Enter для продолжения..."
            Console.ReadLine() |> ignore
            runLoop contacts
        
        | "4" -> // Показать все контакты
            let contactsDisplay = PhoneBook.getAllContactsSimple contacts
            showContacts contactsDisplay
            
            printfn "\nНажмите Enter для продолжения..."
            Console.ReadLine() |> ignore
            runLoop contacts
        
        | "5" -> // Сохранить в файл
            printf "Введите имя файла (например, contacts.txt): "
            let filename = Console.ReadLine()
            
            match PhoneBook.saveToFile contacts filename with
            | Ok () ->
                printfn "анные сохранены в файл '%s'" filename
            | Error error ->
                showError error
            
            printfn "\nНажмите Enter для продолжения..."
            Console.ReadLine() |> ignore
            runLoop contacts
        
        | "6" -> // Загрузить из файла
            printf "Введите имя файла: "
            let filename = Console.ReadLine()
            
            match PhoneBook.loadFromFile filename with
            | Ok loadedContacts ->
                printfn "Загружено %d контактов" (List.length loadedContacts)
                runLoop loadedContacts
            | Error error ->
                showError error
                printfn "\nНажмите Enter для продолжения..."
                Console.ReadLine() |> ignore
                runLoop contacts
        
        | "0" -> // Выход
            printfn "До свидания!"
        
        | _ -> // Неверный выбор
            printfn "Неверный выбор. Пожалуйста, выберите 0-6"
            printfn "Нажмите Enter для продолжения..."
            Console.ReadLine() |> ignore
            runLoop contacts
 module Program =
    [<EntryPoint>]
    let main argv =
        Console.OutputEncoding <- System.Text.Encoding.UTF8
        printfn "Добро пожаловать в телефонный справочник!"
        UserInterface.runLoop []
        0