import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { Component, Input, OnInit } from '@angular/core';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { CartCardComponent } from '../../Components/cart-card/cart-card.component';
import { delay, Observable, Subscription } from 'rxjs';
import { CartService } from '../../Services/cart.service';
import { CartItem } from '../../Models/cart-item';
import { GuidService } from '../../Services/guid.service';
import { CookieService } from 'ngx-cookie-service';
import { LoadingService } from '../../Services/loading-service.service';
import { ProductService } from '../../Services/product.service';

@Component({
  selector: 'app-cart',
  standalone: true,
  imports: [RouterModule, CommonModule, CartCardComponent],
  templateUrl: './cart.component.html',
  styleUrl: './cart.component.css',
})
export class CartComponent implements OnInit {
  cartItems: any[] | null = [];
  Qnt: number = 0;
  subCart: Subscription | null = null;
  loading: boolean = true;
  // isLoading$ : Observable<boolean> | undefined;
  // isAuthenticated:boolean;

  constructor(
    public http: HttpClient,
    public activatedRoute: ActivatedRoute,
    private cartService: CartService,
    public guidServices: GuidService,
    public cookieService: CookieService,
    private loadingService: LoadingService,
    private productService : ProductService
  ) {}

  ngOnInit(): void {
    // if(localStorage.getItem('isAuthenticated'))
    // {
    //   this.isAuthenticated = true;
    // }
    // else{
    //   this.isAuthenticated = false;
    // }
    // this.isLoading$= this.loadingService.loading$
    this.loadingService.show();
        this.subCart = this.cartService.cartQnt.subscribe({
      next: (p) => {
        this.Qnt = p;
      },
    });
    if (this.cookieService.get('Qnt') != null) {
      this.Qnt = Number(this.cookieService.get('Qnt'));
    } else {
      this.Qnt = 0;
    }

    this.subCart = this.cartService.cartProduct$.subscribe({
      next: (products) => {
        if (products.length != 0) {
          console.log('from list');
          this.cartItems = products;
          console.log('Updated cartProducts:', this.cartItems);
          this.loadingService.hide();
          console.log('Cart data loaded:', products);
        } else {
          console.log('from database');
          this.subCart = this.activatedRoute.params.subscribe((params) => {
            this.loadingService.show();
            this.cartService.getAllFromCart(params['cartId']).subscribe({
              next: (cart) => {
                if (cart != null) {
                  this.cartItems = cart.items;
                  this.cartService.updateCart(cart.items);
                  console.log('Updated cartProducts:', this.cartItems);
                  this.loadingService.hide();
                  console.log('Cart data loaded:', cart);
                } else {
                  this.cartItems = null;
                  this.loadingService.hide();
                }
              },
              error: (err) => {
                this.loadingService.show();
                console.error('Error fetching cart data:', err);
              },
            });
          });
        }
      },
      error: (err) => {
        this.loadingService.show();
        console.error('Error fetching cart data:', err);
      },
    });
  }

  ngOnDestroy(): void
  {
    if (this.subCart)
      {
      this.subCart.unsubscribe();
    }
  }

  getGuid(): string
  {
    return this.guidServices.getGUID();
  }

  RemoveFromCart(cartId: string, cartItamId: number): void {
    if (this.cartItems.length > 1) {
      this.cartService.removeFromCart(cartId, cartItamId);
    } else {
      this.cartService.removeCart(cartId);
    }
  }

  UpdateQnt(cartId: string, item: CartItem): void
  {
    this.productService.getProductById(item.id).subscribe({
      next: p =>{
        this.cartService.updateCartItemQnt(cartId, item,p.quantityInStock);
      }
    })
    console.log(item.quantity);
    this.loading = false;
  }

  TotalPrice()
  {
    let TotalPrice = 0;
    if (this.cartItems != null)
    {
      this.cartItems.forEach((item) => {
        TotalPrice += Number(item.price * item.quantity);
      });
      // this.cookieService.set("cart-total-price", TotalPrice.toFixed(2), { expires: 7, path: '/' });
      return TotalPrice.toFixed(2);
    }
    // this.cookieService.set("cart-total-price", TotalPrice.toFixed(2), { expires: 7, path: '/' });
    return TotalPrice.toFixed(2);
  }
}
