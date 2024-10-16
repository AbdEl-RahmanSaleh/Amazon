import { Routes } from '@angular/router';
import { SearchComponent } from './Pages/Search/search.component';
import { HomeComponent } from './Pages/Home/home.component';
import { CategoryComponent } from './Pages/category/category.component';
import { ProductComponent } from './Pages/Product/product.component';
import { CartComponent } from './Pages/cart/cart.component';
import { WishListComponent } from './Pages/wish-list/wish-list.component';
import { OrderComponent } from './Pages/order/order.component';

export const App_Routes: Routes = 
[
    { path: '', component: HomeComponent },
    { path: 'search/:productName', component: SearchComponent },
    { path: 'category/:ParentCategoryName', component: CategoryComponent },
    { path: 'category/:ParentCategoryName/:categoryName', component: CategoryComponent },
    { path: 'product/:id',component:ProductComponent},
    { path: 'cart/:cartId',component:CartComponent},
    { path: 'wishlist/:wishlistId',component:WishListComponent},
    { path: 'order',component:OrderComponent}
];
