# ODataFilterProperties
An example project which allows you to filter the properties on the API per user or whatever rules you want to use.

The important parts of this can be found in `WebAPIConfig.cs` When you are building out your OData model you can configure what properties are hidden as well.
The `key` parameter here is just used for debugging. The important part is what lambda you pass into `AddFilter` on the EntitySet.

To test you can curl this:
```
curl -X GET \
  'http://localhost:54647/odata/Users?%24select=fullName%2Cemail%2CuserName&%24expand=enrollments(%24expand%3Dcourse)' \
  -H 'cache-control: no-cache' \
  -H 'postman-token: df34652c-bbd9-a40e-0187-6222136fd9b5'
```

To show fields for the "Administrator" role, call this to get the bearer token:
```
curl -X POST \
  http://localhost:54647/Token \
  -H 'cache-control: no-cache' \
  -H 'postman-token: 3ce363ba-cac2-511e-462f-58d259e4de2f' \
  -d 'grant_type=password&username=admin&password=password'
```

Then add the bearer token to your query like this:
```
curl -X GET \
  'http://localhost:54647/odata/Users?%24select=fullName%2Cemail%2CuserName&%24expand=enrollments(%24expand%3Dcourse)' \
  -H 'authorization: Bearer eL_AoGDjCjqapqYVGGXCcX7etXlyte1vfp7wD2A879U3vfzwzXT0-FcDIWyb2liah57AedRnWTl5B3cC-a1kAi0UVTMQuYVMeDqj-goYxDk-7hIWeiMd6iSTxkp3lpdcoR9fM_7poOljGrXUdxkQxeeq5kUKxknnhe951ugIc5iiu3ch5nf-1SGjy33_Ybs8DmftE_jhUsrYflp8jkZp1MYBW0zAarF6RrQH03h0bfBqlIWEHI7yZKvYTVbjLUxgzfyUsOyLgkaQ40_hMtiTwgQ3R12BS1tf6WrA8UtgUaVFdu6ch3IdkrWkOg-MCzr_2QfrxIssm_EYRzUbH5dno6T2ME-1lAfQW2aJ8G7h7wZ8TLOa-ocCQU8SuICu8CuJfm-GFRJ3DuZhXGJYXu4KoWv0OyV54aGBM0XQ6I1yx4ekM_ZmlGx1QSugvrbROkFWUY632zUK0mBeDQosHsUnkJm57KbENPR_kzIC4lUuOUmIxgKY-BHmyoiI4eLBpgFybyK5IrkM3XseEx6flQVC9A' \
  -H 'cache-control: no-cache' \
  -H 'postman-token: d5511524-5a26-5cd8-3a91-a7ebae2b502b'
```
