using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace PureBlazor.AsyncExtensions;

/// <summary>
/// Renders a list of items asynchronously. Add [StreamRendering(true)] to the component to enable streaming.
/// 
/// Optionally provide a template for rendering each item.
/// </summary>
/// <typeparam name="T">The type of item</typeparam>
/// <typeparam name="TOrderKey">The key of the item for ordering</typeparam>
public class AsyncRenderer<T, TOrderKey> : ComponentBase, IAsyncDisposable
{
    private CancellationTokenSource cts = new();
    private Task? streamTask;
    private List<T> items = [];
    private IEnumerable<T> OrderedItems => Ordered();

    /// <summary>
    /// The items to render.
    /// </summary>
    [Parameter] public required IAsyncEnumerable<T> Items { get; set; }
    
    /// <summary>
    /// Optional template for rendering each item.
    /// </summary>
    [Parameter] public RenderFragment<T>? ItemTemplate { get; set; }
    
    /// <summary>
    /// The cancellation token to use when enumerating the items.
    /// </summary>
    [Parameter] public CancellationToken? CancellationToken { get; set; }
    
    /// <summary>
    /// The element to render for each item. Defaults to 'div'.
    /// </summary>
    [Parameter] public string Element { get; set; } = "div";

    /// <summary>
    /// The OrderBy function to use when ordering the items. Must not be specified with OrderByDescending.
    /// </summary>
    [Parameter]
    public Func<T, TOrderKey>? OrderBy { get; set; }

    /// <summary>
    /// The OrderByDescending function to use when ordering the items. Must not be specified with OrderBy.
    /// </summary>
    [Parameter]
    public Func<T, TOrderKey>? OrderByDescending { get; set; }

    protected override Task OnInitializedAsync()
    {
        if (CancellationToken is not null)
        {
            cts = CancellationTokenSource.CreateLinkedTokenSource(CancellationToken.Value);
        }

        if (ItemTemplate is null)
        {
            // Create a default template that just calls ToString on the item
            ItemTemplate = item => (builder) => builder.AddContent(0, item);
        }

        streamTask = Task.Run(() => Enumerate(cts.Token), cts.Token);
        return Task.CompletedTask;
    }

    private IEnumerable<T> Ordered()
    {
        if (OrderBy is not null && OrderByDescending is not null)
        {
            throw new InvalidOperationException("Must not specify both OrderBy and OrderByDescending");
        }

        if (OrderBy is not null)
        {
            return items.OrderBy(OrderBy);
        }

        if (OrderByDescending is not null)
        {
            return items.OrderByDescending(OrderByDescending);
        }

        return items;
    }

    private async Task Enumerate(CancellationToken ct = default)
    {
        items = [];

        await foreach (var item in Items.WithCancellation(ct))
        {
            items.Add(item);
            await InvokeAsync(StateHasChanged);
        }
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        foreach (var item in OrderedItems)
        {
            builder.OpenElement(0, Element);

            // ItemTemplate not null after initializing
            builder.AddContent(1, ItemTemplate!(item));
            builder.CloseElement();
        }
    }

    public ValueTask DisposeAsync()
    {
        cts.CancelAsync();
        streamTask?.Dispose();

        GC.SuppressFinalize(this);

        return ValueTask.CompletedTask;
    }
}