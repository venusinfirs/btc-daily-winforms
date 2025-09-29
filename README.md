**BtcDaily: Cryptocurrency Price Viewer**

BtcDaily is a lightweight Windows desktop application built with C# WinForms designed to provide a simple, interactive, line-chart visualization of cryptocurrency price history. The application uses a Layered Architecture to ensure clean separation of concerns, maintainability, and extensibility.

**Current Features**
Bitcoin (BTC) Price Chart: Displays a line chart of BTC's price.

Fixed Time Range: Currently displays price history for the last 24 hours.

External Data Source: Price data is reliably fetched from the CoinGecko API.

Interactive UI: Includes dynamic chart styling (color coding for gains/losses) and data point tooltips.

**Future Expansion & Development Plans**
1. Core Feature Expansion
Multiple Currencies: Implement UI controls to allow users to select and display charts for various cryptocurrencies (e.g., Ethereum, Solana) by leveraging the existing Currency entity and PriceService.

Flexible Time Ranges: Expand the available timeframes (e.g., 7 days, 3 months, 1 year) by utilizing the TimeRange enum and updating the API fetching logic within the infrastructure layer.

2. Architectural Improvements
Alternative Price Providers: integrate alternative price providers beyond the current CoinGeckoPriceFetcher. This will involve adding new implementations of the IPriceFetcher interface.

Dependency Injection (DI): Migrate the manual service instantiation in the main form (Form1) to use an Inversion of Control (IoC) container. This will simplify service management, enhance testability, and adhere to the Dependency Inversion Principle.

**Technology Stack**
Platform: C# (.NET)

UI Framework: Windows Forms (WinForms)

Charting: System.Windows.Forms.DataVisualization.Charting

External API: CoinGecko

JSON Handling: Newtonsoft.Json
