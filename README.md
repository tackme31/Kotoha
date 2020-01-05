# Kotoha
*Kotoha* is a Sitecore module for generating an efficient query of a keyword search that supports field-level boosting.  

**This software is in early stage of development.**

## Installation
*Kotoha* is not available in NuGet Gallery yet. Download `.nupkg` file from [here](https://github.com/xirtardauq/Kotoha/releases) and install it locally.  

## Usage
You can see a sample configuration in [Kotoha.config.example](./Kotoha/App_Config/Include/Kotoha/Kotoha.config.example).

1. Add a configuration for a keyword search target.

```xml
<configuration>
  <sitecore>
    <kotoha>
      <configuration type="Kotoha.KeywordSearchConfiguration, Kotoha">
        <searchTargets hint="list:AddSearchTarget">
          <searchTarget id="blog" type="Kotoha.KeywordSearchTarget, Kotoha">
            <!--
                Identifier of this search target.
                This must be unique across search targets.
            -->
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
<configuration>
  <sitecore>
    <contentSearch>
      <configuration type="Sitecore.ContentSearch.ContentSearchConfiguration, Sitecore.ContentSearch">
        <indexes search:require="solr">
          <!-- Of course, this should be set to the sitecore_web_index index. -->
          <index id="sitecore_master_index" type="Sitecore.ContentSearch.SolrProvider.SolrSearchIndex, Sitecore.ContentSearch.SolrProvider">
            <configuration ref="contentSearch/indexConfigurations/defaultSolrIndexConfiguration">
              <documentOptions ref="contentSearch/indexConfigurations/defaultSolrIndexConfiguration/documentOptions">
                <fields hint="raw:AddComputedIndexField">
                  <!-- The 'searchTargetId' attribute must be set the search target's ID that configures in the previous step. -->
                  <field fieldName="ks_blog" returnType="text" searchTargetId="blog">Kotoha.KeywordSearchContentIndexField, Kotoha</field>
                </fields>
              </documentOptions>
            </configuration>
          </index>
        </indexes>
      </configuration>
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
        // Create a query by search target ID and keywords
        var query = context.CreateKeywordSearchQuery<SearchResultItem>("blog", keywords);

        query = query.Filter(item => item.TemplateName == "Blog Page");

        return query.OrderByDescending(item => item["score"]).GetResults();
    }
}
```

4. Build and deploy your project.

5. In Sitecore, populate solr managed schema and rebuild all indexes.

6. Test your search code works well.

## See also
- [Implementing keyword search with field-level boosting in Sitecore](https://dev.to/xirtardauq/implementing-a-keyword-search-with-field-level-boosting-in-sitecore-99g)

## Author
- Takumi Yamada (xirtardauq@gmail.com)

## License
*Kotoha* is licensed unther the MIT license. See LICENSE.txt.