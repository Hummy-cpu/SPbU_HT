module Tests

open NUnit.Framework
open PhoneBook.Core
open System.IO

[<TestFixture>]
module ValidateNameTests =
    
    [<Test>]
    let ``возвращает Ok с урезанным именемe`` () =
        let result = PhoneBook.validateName "  Иван  "
        match result with
        | Ok name -> Assert.That(name, Is.EqualTo("Иван"))
        | Error _ -> Assert.Fail()
    
    [<Test>]
    let ``пустое - ошибка`` () =
        let result = PhoneBook.validateName ""
        match result with
        | Error (InvalidName _) -> Assert.Pass()
        | _ -> Assert.Fail()

[<TestFixture>]
module ValidatePhoneTests =
    
    [<Test>]
    let ``только цифвры - ок`` () =
        let result = PhoneBook.validatePhone "123456"
        match result with
        | Ok phone -> Assert.That(phone, Is.EqualTo("123456"))
        | Error _ -> Assert.Fail()
    
    [<Test>]
    let ``не только цифры - Error`` () =
        let result = PhoneBook.validatePhone "123-456"
        match result with
        | Error (InvalidPhone _) -> Assert.Pass()
        | _ -> Assert.Fail()

[<TestFixture>]
module AddContactTests =
    
    let empty = []
    
    [<Test>]
    let ``добавляет новый контакт`` () =
        let result = PhoneBook.addContact empty "Иван" "123456"
        match result with
        | Ok contacts ->
            Assert.That(contacts.Length, Is.EqualTo(1))
            Assert.That(contacts.Head.Name, Is.EqualTo("Иван"))
        | Error _ -> Assert.Fail()
    
    [<Test>]
    let ``возвращает ошибку для дубликата имени`` () =
        let contacts = [{ Name = "Иван"; Phone = "123456" }]
        let result = PhoneBook.addContact contacts "Иван" "789012"
        match result with
        | Error ContactAlreadyExists -> Assert.Pass()
        | _ -> Assert.Fail()
    
    [<Test>]
    let ``ошибка для невалидного телефона`` () =
        let result = PhoneBook.addContact empty "Иван" "abc"
        match result with
        | Error (InvalidPhone _) -> Assert.Pass()
        | _ -> Assert.Fail()

[<TestFixture>]
module FindByNameTests =
    
    let contacts = [
        { Name = "Иван"; Phone = "123456" }
        { Name = "Мария"; Phone = "789012" }
    ]
    
    [<Test>]
    let ``возвращает номер по имени`` () =
        let result = PhoneBook.findByName contacts "Иван"
        match result with
        | Ok phone -> Assert.That(phone, Is.EqualTo("123456"))
        | Error _ -> Assert.Fail()
    
    [<Test>]
    let ``findByName - error для несуществующего контакта`` () =
        let result = PhoneBook.findByName contacts "Петр"
        match result with
        | Error (InvalidName _) -> Assert.Pass()
        | _ -> Assert.Fail()

[<TestFixture>]
module FindByPhoneTests =
    
    let contacts = [
        { Name = "Иван"; Phone = "123456" }
        { Name = "Мария"; Phone = "789012" }
    ]
    
    [<Test>]
    let ``findByPhone возвращает имя по телефону`` () =
        let result = PhoneBook.findByPhone contacts "123456"
        match result with
        | Ok name -> Assert.That(name, Is.EqualTo("Иван"))
        | Error _ -> Assert.Fail()
    
    [<Test>]
    let ``findByPhone возвращает error если не сущ.`` () =
        let result = PhoneBook.findByPhone contacts "999999"
        match result with
        | Error (InvalidPhone _) -> Assert.Pass()
        | _ -> Assert.Fail()

[<TestFixture>]
module GetAllContactsSimpleTests =
    
    [<Test>]
    let ``getAllContactsSimple - пустой список для пустых контактов`` () =
        let result = PhoneBook.getAllContactsSimple []
        Assert.That(result, Is.Empty)
    
    [<Test>]
    let ``getAllContactsSimple возвращает контакты`` () =
        let contacts = [
            { Name = "Борис"; Phone = "222" }
            { Name = "Анна"; Phone = "111" }
        ]
        let result = PhoneBook.getAllContactsSimple contacts
        Assert.That(result.[0], Is.EqualTo("Имя: Анна, Телефон: 111"))
        Assert.That(result.[1], Is.EqualTo("Имя: Борис, Телефон: 222"))

[<TestFixture>]
module SaveToFileTests =
    
    let testFile = "test_save.txt"
    let contacts = [{ Name = "Иван"; Phone = "123456" }]
    
    [<TearDown>]
    let cleanup () =
        if File.Exists(testFile) then
            File.Delete(testFile)
    
    [<Test>]
    let ``saveToFile сохраняет контакты в файл`` () =
        let result = PhoneBook.saveToFile contacts testFile
        match result with
        | Ok () ->
            Assert.That(File.Exists(testFile), Is.True)
            let lines = File.ReadAllLines(testFile)
            Assert.That(lines.[0], Is.EqualTo("Иван|123456"))
        | Error _ -> Assert.Fail()
    
    [<Test>]
    let ``saveToFile возвращает ок для пустых контактов`` () =
        let result = PhoneBook.saveToFile [] testFile
        match result with
        | Ok () -> Assert.Pass()
        | Error _ -> Assert.Fail()

[<TestFixture>]
module LoadFromFileTests =
    
    let testFile = "test_load.txt"
    
    [<TearDown>]
    let cleanup () =
        if File.Exists(testFile) then
            File.Delete(testFile)
    
    [<Test>]
    let ``loadFromFile загружает контакты из файла`` () =
        File.WriteAllLines(testFile, ["Иван|123456"; "Мария|789012"])
        let result = PhoneBook.loadFromFile testFile
        match result with
        | Ok contacts ->
            Assert.That(contacts.Length, Is.EqualTo(2))
            Assert.That(contacts.[0].Name, Is.EqualTo("Иван"))
            Assert.That(contacts.[1].Name, Is.EqualTo("Мария"))
        | Error _ -> Assert.Fail()
    
    [<Test>]
    let ``loadFromFile возвращает error если контактов нет`` () =
        let result = PhoneBook.loadFromFile "nonexistent.txt"
        match result with
        | Error (FileError _) -> Assert.Pass()
        | _ -> Assert.Fail()