Проблема:
==
Joinrpg в настоящий момент имеет UI составленный классическим подходом (ASP.NET MVC, Razor). Это UI очень удобен для быстрого прототипирования, но не поддерживает AJAX.
Для добавления AJAX на страницу приходится писать по месту плохо структурированный код на JS на устаревших древних технологиях (bootstrap, jquery).  
При этом AJAX критичен для работы Joinrpg в некоторых местах
Мы бы хотели попробовать Blazor. При этом мы не имеем возможности переписать весь с проект с нуля

Решение:
==
Создание «островов» функциональности в виде больших виджетов Blazor components, которые добавляют интерактивности в нужных местах.
Blazor исполняется на клиенте (в режиме WebAssemblyPrerendered)
Каждый такой виджет состоит из отдельных маленьких Razor Components (типа ссылка, лейбл, кнопка) которые не несут логики и могут исполнятся как на клиенте (в составе Blazor) так и на сервере (в составе обычных Razor шаблонов)

Подробности
==

Пример компонента
---

https://github.com/joinrpg/joinrpg-net/blob/2794a2be029740970ec199ae30af7b313fe849aa/src/JoinRpg.Web.GameSubscribe/MasterSubscribeList.razor 
Компонент умеет как принимать готовую модель, так и вызывать клиента для ее загрузки. Это необходимо для пререндеринга.
Сейчас компонент умеет рендерится в трех режимах:
Рендерится на сервере в HTML и подставляется в страницу
Принимает готовую модель в параметры и рендерится на клиенте
Принимает id в параметры, вызывает сервер для загрузки данных и рендерится на клиенте
Компонент интерактивный, сам обрабатывает клики

Пример как положить компонент на страницу https://github.com/joinrpg/joinrpg-net/blob/2794a2be029740970ec199ae30af7b313fe849aa/src/JoinRpg.Portal/Views/GameSubscribe/ByMaster.cshtml 

JoinRpg.Blazor.Client
---
Собирает все нужные куски и рендерит на клиенте

Интерфейсы доступа к данным 
---
Пример IGameSubscribeClient. Интерфейс, который может быть реализован два раза:
https://github.com/joinrpg/joinrpg-net/blob/2794a2be029740970ec199ae30af7b313fe849aa/src/JoinRpg.Blazor.Client/ApiClients/GameSubscribeClient.cs - на клиенте, дергает API
https://github.com/joinrpg/joinrpg-net/blob/2794a2be029740970ec199ae30af7b313fe849aa/src/JoinRpg.WebPortal.Managers/Subscribe/SubscribeViewService.cs - на сервере, непосредственный доступ к данным. Используется из контроллеров (API и MVC)

Авторизация
---
Клиент использует те же куки, что и обычное веб-приложение
Чтобы предовратить CSRF, ему в специальной куке присылается стандартный ASP.NET antiforgery token (https://github.com/joinrpg/joinrpg-net/blob/2794a2be029740970ec199ae30af7b313fe849aa/src/JoinRpg.Blazor.Client/ApiClients/GameSubscribeClient.cs 
https://github.com/joinrpg/joinrpg-net/blob/e86cd11cb594969613309f99901b7e845bcd8d5f/src/JoinRpg.Portal/Infrastructure/CsrfTokenCookieMiddleware.cs 

Организация по проектам
---
 - Есть проект Joinrpg.Blazor.Client — собирает весь код в одно место, также содержит реализации ApiClient
 - Есть проекты по фичам, например JoinRpg.Web.GameSubscribe — хранит весь интерактивный код по фичам, может исполнятся на клиенте и на сервере
 - Есть проект JoinRpg.WebComponents — содержит неинтерактивные компонентики, из которых в перспективе можно собрать «фреймворк» стандартных JoinRpg компонентов

Что делать при необходимости использовать готовый Partial
---
Если это Partial без интерактива, необходимо содержимое его перенести в компонент JoinRpg.WebComponents и заменить Partial вызовом статического рендеринга компонента.
Если с интерактивом — скорее всего придется нормально рефакторить, но тогда он вряд ли используется больше чем в одном месте.

Дальнейшие планы
---
 - Шико посмотреть PR (особенно компоненты)
 - Сделать пример как занести Partial в компоненты
 - Реализовать на Blazor стандартный открывающийся диалог. 
 - Использовать его на форме заявки
 - Перенести комменты в Blazor
 