using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace PureBlazor.AsyncExtensions;

/// <summary>
/// Renders a list of items asynchronously. Add [StreamRendering(true)] to the component to enable streaming.
///
/// Optionally provide a template for rendering each item.
/// </summary>
/// <typeparam name="T"></typeparam>
public class AsyncRenderer<T> : ComponentBase, IAsyncDisposable
{
    private CancellationTokenSource _cts = new();
    private Task? _streamTask;
    private List<T> _items = [];

    [Parameter] public required IAsyncEnumerable<T> Items { get; set; }
    [Parameter] public RenderFragment<T>? ItemTemplate { get; set; }
    [Parameter] public CancellationToken? CancellationToken { get; set; }

    protected override Task OnInitializedAsync()
    {
        if (CancellationToken is not null)
        {
            _cts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.Value);
        }

        if (ItemTemplate is null)
        {
            // Create a default template that just calls ToString on the item
            ItemTemplate = item => (builder) => builder.AddContent(0, item);
        }

        _streamTask = Task.Run(Enumerate);
        return Task.CompletedTask;
    }

    private async Task Enumerate()
    {
        _items = [];

        await foreach (var item in Items)
        {
            _items.Add(item);
            await InvokeAsync(StateHasChanged);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "div");
        foreach (var item in _items)
        {
            builder.OpenElement(1, "div");

            // ItemTemplate not null after initializing
            builder.AddContent(2, ItemTemplate!(item));
            builder.CloseElement();
        }
        builder.CloseElement();
    }

    public ValueTask DisposeAsync()
    {
        _cts.CancelAsync();
        _streamTask?.Dispose();

        return ValueTask.CompletedTask;
    }
}