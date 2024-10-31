import { Component, OnInit } from '@angular/core';
import { ProductService } from '../../../../Services/product.service';
import { Product } from '../../../../Models/product';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CommonModule } from '@angular/common';
import { ReviewService } from '../../../../Services/review.service';
import { Review } from '../../../../Models/review';
import { Subscription } from 'rxjs';
import { ReactiveFormsModule } from '@angular/forms';
import { SellerService } from '../../../../Services/seller.service';
import { OrderService } from '../../../../Services/order.service';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-admin-product-list',
  standalone: true,
  imports: [RouterModule, CommonModule, ReactiveFormsModule],
  templateUrl: './seller-product-list.component.html',
  styleUrl: './seller-product-list.component.css',
})
export class SellerProductListComponent implements OnInit {
  products: Product[] = []; // Initialized as an empty array
  isAuthenticated: boolean = false; // Initialized to false
  userName: string | null = null; // Initialized to null
  selectedProductName: string | null = null;
  selectedProductId: number | null = null;
  allSellerEarnings: any | 0 = 0;
  allProductsReviews: any | 0 = 0;
  SoldNumber: any | 0 = 0;
  averagePrice: any | 0 = 0;
  sub: Subscription | null = null;
  constructor(
    private router: Router,
    private sellerService: SellerService,
    private reviewService: ReviewService,
    public toastr: ToastrService
  ) {}

  ngOnInit(): void {
    const isAuthenticated = localStorage.getItem('isAuthenticated');
    this.isAuthenticated = isAuthenticated
      ? JSON.parse(isAuthenticated)
      : false; // Handle potential null
    this.userName = localStorage.getItem('userName');

    this.loadSellerProducts(); // Method to load products
    this.loadSellerStats();
  }
  loadSellerStats(): void {
    let earning = {
      totalEarnings: 0,
    };
    this.sub = this.sellerService.GetAllSellerEarnings().subscribe({
      next: (data) => {
        console.log(' seller products earning:', data);
        earning = data;
        this.allSellerEarnings = earning.totalEarnings;
      },
      error: (error) => {
        console.error('Error loading seller products:', error);
      },
    });
    this.sellerService.GetSellerAcceptedProducts().subscribe({
      next: (datas) => {
        datas.forEach((elm) =>{
          this.averagePrice +=elm.price;
        })
        this.averagePrice =this.averagePrice / datas.length
        datas.forEach((elm) => {
          this.reviewService
            .getAllProductReviewsById(elm.id)
            .subscribe((datar) => {
              this.allProductsReviews += datar.length;
            });
        });
      },
      error: (error) => {
        console.error('Error loading seller products:', error);
      },
    });
    this.sellerService.GetAllSellerEarningsWithDetails().subscribe({
      next: (data) => {
        console.log('asdadadadadaadsadd:', data);
        data.forEach((elm) =>{
          this.SoldNumber +=elm.quantitySold;
        })

      },
      error: (error) => {
        console.error('Error loading seller products:', error);
      },
    });
  }
  loadSellerProducts(): void {
    this.sub = this.sellerService.GetSellerProducts().subscribe({
      next: (data: Product[]) => {
        console.log(data);
        this.products = data; // Assuming data is of type Product[]
      },
      error: (error) => {
        console.error('Error loading seller products:', error);
      },
    });
  }

  add(product: Product): void {
    const serializedObject = encodeURIComponent(JSON.stringify(product));
    this.router.navigate(['/seller/edit-product', serializedObject]);
  }
  edit(product: Product): void {
    const serializedObject = encodeURIComponent(JSON.stringify(product));
    this.router.navigate(['/seller/edit-product', serializedObject]);
  }
  selectProductName(Name: string) {
    this.selectedProductName = Name;
  }
  selectProduct(id: number) {
    this.selectedProductId = id;
  }

  // deleteProduct(id: number): void {
  //   this.sellerService.DeleteProduct(id).subscribe({
  //     next: () => {
  //       this.products = this.products.filter((product) => product.id !== id);
  //       this.toastr.success('Product Delete Successfully');
  //     },
  //     error: (error) => {
  //       console.log(error);
  //       this.toastr.error('Can\'t Delete this product, still delivering ', 'Can\'t Delete', {
  //         positionClass: 'toast-bottom-right',
  //     });
  //   },
  //   });
  // }

  deleteProduct(id: number): void {
  this.sellerService.DeleteProduct(id).subscribe({
    next: () => {
      this.products = this.products.filter((product) => product.id !== id);
      this.toastr.success('Product Deleted Successfully');
    },
    error: (error) => {
      console.error(error);
      this.toastr.error("Can't Delete this product, still delivering", "Can't Delete", {
        positionClass: 'toast-bottom-right',
      });
    },
  });
}

  signOut(): void {
    localStorage.clear();
    this.router.navigate(['/login']); // Redirect to login using the router
  }
}
