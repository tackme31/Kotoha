# Kotoha
*Kotoha* is a Sitecore module for generating an efficient query of a keyword search that supports field-level boosting.  

**This software is in early stage of development.**

## Installation
WIP

## Usage
1. Enable `Kotoha.KeywordSearchConfiguration.config.example` and edit as follows.

```xml
<?xml version="1.0" encoding="utf-8" ?>
<configuration xmlns:role="http://www.sitecore.net/xmlconfig/role/">
  <sitecore role:require="Standalone or ContentManagement or ContentDelivery" >
    <kotoha>
      <KeywordSearchConfiguration type="Kotoha.KeywordSearchConfiguration, Kotoha">
        <!--
            The field name that contains concatenated values of the target fields.
            If the 'kscontent' field is in other use, you need to change this value and predefined field name in Kotoha.config.
        -->
        <param name="contentFieldName">kscontent</param>
        <!--
            Target fields and its boosting values of a keyword search.
            If you want to use a field without boosting, remove the 'boost' attribute or specify 0 to that's value.
        -->
        <targetFields hint="raw:AddTargetField">
          <field name="Title"             boost="9" />
          <field name="Description"       boost="5" />
          <field name="Tags"              boost="4" />
          <field name="Body"              boost="4" />
          <field name="Category"          boost="2" />
          <field name="Author"            boost="2" />
          <field name="Html Title"                  />
          <field name="Meta Description"            />
          <field name="Meta Keywords"               />
        </targetFields>
      </KeywordSearchConfiguration>
    </kotoha>
  </sitecore>
</configuration>
```

2. Implement your search code. The following code is just a sample.

```cs
public SearchResults<SearchResultItem> SearchByKeywords(ICollection<string> keywords)
{
    // Get a service for generating a keyword search query.
    var service = ServiceLocator.ServiceProvider.GetService(typeof(IKeywordSearchQueryService)) as IKeywordSearchQueryService;

    var index = ContentSearchManager.GetIndex("sitecore_master_index");
    using (var context = index.CreateSearchContext())
    {
        var query = context
            .GetQueryable<SearchResultItem>()
            .Filter(item => item.Paths.Contains(ItemIDs.ContentRoot));

        // Apply a keyword search condition to the query.
        query = service.ApplyKeywordSearchQuery(query, keywords);

        return query
            .OrderByDescending(item => item["score"])
            .GetResults();
    }
}
```

3. Build and deploy your project to website.

1. In Sitecore, populate solr managed schema and rebuild all indexes.

1. Run and check your search works well. 

## Author
- Takumi Yamada (xirtardauq@gmail.com)

## License
*Kotoha* is licensed unther the MIT license. See LICENSE.txt.