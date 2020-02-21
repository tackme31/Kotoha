# Kotoha
*Kotoha* is a Sitecore library for generating an efficient query of keyword search that supports field-level boosting.  

**This software is in early stage of development.**

## Supports
- Sitecore: XM/XP 9.3
- Search Provider: Solr/Azure Search

## Installation
*Kotoha* is not available in NuGet Gallery yet. Download `.nupkg` file from [here](https://github.com/xirtardauq/Kotoha/releases) and install it locally.  

## Usage
You can see a sample configuration in [Kotoha.Solr.config.example](./Kotoha/App_Config/Include/Kotoha/Kotoha.Solr.config.example) and [Kotoha.Cloud.config.example](./Kotoha/App_Config/Include/Kotoha/Kotoha.Cloud.config.example).

1. Add a configuration for a keyword search target.

```xml
<configuration xmlns:search="http://www.sitecore.net/xmlconfig/search/">
  <sitecore>
    <kotoha>
      <configuration type="Kotoha.KeywordSearchConfiguration, Kotoha">
        <searchTargets hint="list:AddSearchTarget">
          <searchTarget id="blog" type="Kotoha.KeywordSearchTarget, Kotoha">
            <!-- A identifier of this search target. This must be unique across search targets. -->
            <param desc="id">$(id)</param>
            <!--
              Target fields and its boosting values of a keyword search.
              If you want to use a field without boosting, remove the boost attribute or specify 0 to that's value.
            -->
            <fields hint="raw:AddField">
              <field name="Title"             boost="5" />
              <field name="Tags"              boost="4" />
              <field name="Body"              boost="4" />
              <field name="Category"          boost="2" />
              <field name="Author"            boost="2" />
              <field name="Html Title"                  />
              <field name="Meta Description"            />
              <field name="Meta Keywords"               />
            </fields>
          </searchTarget>
        </searchTargets>
      </configuration>
    </kotoha>
  </sitecore>
</configuration>
```

2. Add a computed field for the search target and set the `searchTargetId` which points to your search target.

```xml
<configuration xmlns:search="http://www.sitecore.net/xmlconfig/search/">
  <sitecore>
    <contentSearch>
      <indexConfigurations>
        <!-- Solr -->
        <defaultSolrIndexConfiguration search:require="solr">
          <documentOptions>
            <fields hint="raw:AddComputedIndexField">
              <!-- A computed field for keyword search. Set the search target's ID to the 'searchTargetId' attribute. -->
              <field fieldName="ks_blog" returnType="text" searchTargetId="blog">Kotoha.KeywordSearchContentIndexField, Kotoha</field>
            </fields>
          </documentOptions>
        </defaultSolrIndexConfiguration>

        <!-- Azure -->
        <defaultCloudIndexConfiguration  search:require="azure">
          <documentOptions type="Sitecore.ContentSearch.DocumentBuilderOptions, Sitecore.ContentSearch">
            <fields hint="raw:AddComputedIndexField">
              <!-- A computed field for keyword search. Set the search target's ID to the 'searchTargetId' attribute. -->
              <field fieldName="ks_blog" searchTargetId="blog" type="Kotoha.KeywordSearchContentIndexField, Kotoha"  />
            </fields>
            <!-- 
              NOTE: When you use Azure Search and indexAllFields is setting to false,
                    the boosted fields have to be added to index.
            -->
            <include hint="list:AddIncludedField">
              <__Bucketable>{C9283D9E-7C29-4419-9C28-5A5C8FF53E84}</__Bucketable>
              <Title>{81E9FCD9-9806-40A5-90CA-3365DE80D3FF}</Title>
              <Tags>{34D69283-63AC-4E38-B39B-88FB7C521955}</Tags>
              <Body>{C6C8B721-6C6C-49D3-87EC-C16C43C61826}</Body>
              <Category>{E7956EAD-CCBB-49B9-A982-A835A7FD44E3}</Category>
              <Author>{81826AE3-B77C-4685-B5AF-6A791C1F9BD2}</Author>
            </include>
          </documentOptions>
        </defaultCloudIndexConfiguration>
      </indexConfigurations>
    </contentSearch>
  </sitecore>
</configuration>
```

3. Implement your search code. The following code is just a sample.

```csharp
public SearchResults<SearchResultItem> SearchBlogByKeywords(string[] keywords)
{
    var index = ContentSearchManager.GetIndex("sitecore_master_index");
    using (var context = index.CreateSearchContext())
    {
        // Create a query for keyword search.
        var query = context.CreateKeywordSearchQuery<SearchResultItem>(searchTargetId: "blog", keywords: keywords);

        // You should add more filters and pagination queries.
        query = query.Filter(item => item.TemplateName == "Blog Page");

        return query.OrderByDescending(item => item["score"]).GetResults();
    }
}
```

4. Build and deploy your project.

5. In Sitecore, populate solr managed schema and rebuild all indexes.

6. Check your search code works well.

### Keyword Search Options
*Kotoha* supports AND/OR search and Contains/Equals conditions. This behavior can be changed by supplying `KeywordSearchOptions`.

```cs
// OR search + Equals condition
var options = new KeywordSearchOptions
{
    SearchType = SearchType.Or,
    Condition = Condition.Equals
};

var query = context.CreateKeywordSearchQuery<SearchResultItem>("blog", keywords, options);
```

The default behavior can be set in the configuration.

```xml
<configuration xmlns:role="http://www.sitecore.net/xmlconfig/role/" xmlns:search="http://www.sitecore.net/xmlconfig/search/">
  <sitecore role:require="Standalone or ContentManagement or ContentDelivery" search:require="solr">
    <kotoha>
      <configuration type="Kotoha.KeywordSearchConfiguration, Kotoha">
        <!-- A KeywordSearchOptions that is used when no options supplied. -->
        <defaultKeywordSearchOptions type="Kotoha.KeywordSearchOptions, Kotoha">
          <SearchType>And</SearchType>
          <Condition>Contains</Condition>
        </defaultKeywordSearchOptions>
      </configuration>
    </kotoha>
  </sitecore>
</configuration>
```

If the no default options configured, *Kotoha* uses AND search and Contains condition.

## See also
- [Search result boosting](https://doc.sitecore.com/developers/93/platform-administration-and-architecture/en/search-result-boosting.html)
- [Implementing keyword search with field-level boosting in Sitecore](https://dev.to/xirtardauq/implementing-a-keyword-search-with-field-level-boosting-in-sitecore-99g)

## Author
- Takumi Yamada (xirtardauq@gmail.com)

## License
*Kotoha* is licensed unther the MIT license. See LICENSE.txt.
