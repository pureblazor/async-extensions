# async-extensions
async extensions for blazor

## AsyncRenderer
Enables rendering of a list of items asynchronously. The items are rendered as they are received from the server.

Add `[StreamRendering(true)]` to the page component to enable streaming rendering.

### Basic usage

The default implementation of `AsyncRenderer` will render the items as a list of divs. If no `ItemTemplate` is provided, the `ToString` method will be called on each item.
```razor
<AsyncRenderer Items="items" />
```

### Use in a table with a custom template
```razor
<table>
    <thead>
        <tr>
            <th>Item</th>
        </tr>
    </thead>
    <tbody>
        <AsyncRenderer Items="items" Element="tr">
            <ItemTemplate>
                <td>@context.Name</td>
            </ItemTemplate>
        </AsyncRenderer>
    </tbody>
</table>
```

### Use in a table with a custom template and ordering (ascending)
```razor
<table>
    <thead>
        <tr>
            <th>Item</th>
        </tr>
    </thead>
    <tbody>
        <AsyncRenderer Items="items" Element="tr" T="Person" TOrderKey="string" OrderBy="p => p.Name">
            <ItemTemplate>
                <td>@context.Name</td>
            </ItemTemplate>
        </AsyncRenderer>
    </tbody>
</table>
```

### Use in a table with a custom template and ordering (descending)
```razor
<table>
    <thead>
        <tr>
            <th>Item</th>
        </tr>
    </thead>
    <tbody>
        <AsyncRenderer Items="items" Element="tr" T="Person" TOrderKey="string" OrderByDescending="p => p.Name">
            <ItemTemplate>
                <td>@context.Name</td>
            </ItemTemplate>
        </AsyncRenderer>
    </tbody>
</table>
```