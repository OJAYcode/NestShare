# Account Balance Menu Additions

## Desktop Menu - Add after Reports link

```html
<a
  asp-controller="AccountBalance"
  asp-action="Index"
  class="px-3 py-2 rounded-md text-sm font-medium text-gray-700 hover:text-gray-900 hover:bg-gray-50 flex items-center"
>
  <i class="fas fa-wallet mr-2 text-purple-500"></i>
  Account Balance
</a>
```

## Mobile Menu - Add after Reports link

```html
<a
  asp-controller="AccountBalance"
  asp-action="Index"
  class="flex items-center px-3 py-2 rounded-md text-base font-medium text-gray-700 hover:text-gray-900 hover:bg-gray-50"
>
  <i class="fas fa-wallet w-6 text-purple-500"></i>
  Account Balance
</a>
```

## Also check for and fix null reference errors by updating:

```html
<span class="text-sm text-gray-700"
  >Hello, @(string.IsNullOrEmpty(username) ? "User" : username)!</span
>
```

Add these changes manually to the \_Layout.cshtml file at the appropriate locations.
