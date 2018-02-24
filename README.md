# Azure AD B2C 获取组信息

因为Azure AD B2C不能直接返回安全组信息，这里提供一种解决方案。

1. 创建一个Azure AD应用，并授予读取目录的权限。
2. 通过Azure AD的权限来读取目录中的安全组信息，再利用Graph API获取某一用户所属的安全组。

参考了微软的[B2C-GraphAPI-DotNet](https://github.com/AzureADQuickStarts/B2C-GraphAPI-DotNet),把查找安全组操作封装了一下。

**Example**:

```c#
// 名称空间为PFStudio.B2C
B2CGraphClient client = new B2CGraphClient("your clientId", "your clientSecret", "your tenant");

IEnumerable<string> groups = await client.GetMemberOf("user's objectId");
```