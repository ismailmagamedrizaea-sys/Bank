 # Avalonia EntityFramework SQLite Project

 ## Как склонировать проект?

 Через граф. интерфейс - нажмите на кнопку **Code** и в выпадающем окне выберите
 **Download ZIP**, после разархивируйте папку куда-нибудь в удобное место.

 Через терминал - пропишите команду:
 ```powershell
 git clone https://github.com/TopPrepod05/avaloniaef-p511
 ```

 ## Что делать после клонирования?

1. Зайти в папку и открыть файл ```Название проекта.slnx```
2. Открыть Developer PowerShell
3. В Developer PowerShell вписать команду: 
	 ```powershell
		dotnet restore
	 ```
4. Вписать вторую команду: 
	```powershell
		dotnet ef database update
	 ```
5. Запустить и проверить работоспособность.



Если не работает какая-то из команд - проверьте правильность написания команд, либо
проверьте, установлен ли **dotnet-ef**. Его установить можно, прописав ```dotnet tool install --global dotnet-ef```

## ВАЖНО

Версия .NET 10. 

