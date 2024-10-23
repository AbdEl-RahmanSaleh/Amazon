import { HttpClient } from '@angular/common/http';
import { Component } from '@angular/core';
import { FormBuilder, FormGroup, FormsModule, ReactiveFormsModule, Validators } from '@angular/forms';
import { ActivatedRoute, Router, RouterModule } from '@angular/router';
import { CategoryService } from '../../Services/category.service';
import { BrandService } from '../../Services/brand.service';
import { Product } from '../../Models/product';
import { Subscription } from 'rxjs';
import { ProductService } from '../../Services/product.service';
import { Discount } from '../../Models/Discount';

@Component({
  selector: 'app-seller-add-product',
  standalone: true,
  imports: [FormsModule,ReactiveFormsModule,RouterModule],
  templateUrl: './seller-edit-product.component.html',
  styleUrl: './seller-edit-product.component.css'
})
export class SellerEditProductComponent {

  productForm: FormGroup;
  product:Product;
  categories = [];  // Load these from API
  brands = [];      // Load these from API
  mainImage: any = null;
  additionalImages: any[] = [];
  discount:Discount | null = new Discount(0, 0, false, null, null);
  sub: Subscription | null = null;
  constructor(public route: ActivatedRoute,private router: Router,public productService:ProductService ,private fb: FormBuilder, private http: HttpClient,public categoryService:CategoryService,public brandService:BrandService) {}
  
  ngOnInit(): void {
    this.sub = this.route.params.subscribe(params => {
      if(params['product']){
        this.product = JSON.parse(decodeURIComponent(params['product']));
        if(this.product.discount){
          this.discount = this.product.discount;
        }
        else{
          this.discount= new Discount(0, 0, false, null, null);
        }
        
      }
    })
    this.productForm = this.fb.group({
      id: this.product.id,
      name: ['', Validators.required],
      description: ['', Validators.required],
      price: [0, Validators.required],
      quantityInStock: [0, Validators.required],
      brandId: ['', Validators.required],
      categoryId: ['', Validators.required],
      imageFile: [null],
      imagesFiles: [null],
      discount:null
    });


    // Load categories and brands from the backend
    this.loadCategories();
    this.loadBrands();
  }

  loadCategories() {
    this.categoryService.getCategories().subscribe((data: any) => {
      this.categories = data;
    });
  }

  loadBrands() {
    this.brandService.getBrands().subscribe((data: any) => {
      this.brands = data;
    });
  }

  onFileSelect(event: any, fileType: string) {
    if (fileType === 'mainImage') {
      this.mainImage = event.target.files[0];
    } else if (fileType === 'additionalImages') {
      this.additionalImages = event.target.files;
    }
  }

  addDiscount() {
    // Ensure discount percentage is within valid range
    if (this.discount.discountPercentage < 0 || this.discount.discountPercentage > 1) {
      alert('Discount percentage must be between 0 and 1.');
      return;
    }

    // Ensure start date and end date are valid
    if (new Date(this.discount.startDate) > new Date(this.discount.endDate)) {
      alert('Start date cannot be later than end date.');
      return;
    }


    this.product!.discount = this.productForm.get('discount')?.value || this.product!.discount;

    // Here you can call the backend service to save the discount
    // Example: this.discountService.addDiscount(this.discount).subscribe(...)

    console.log('Discount added:', this.discount);


  }


  resetForm() {
    this.discount  = {
      priceAfterDiscount:0,
      discountPercentage: 0.0,
      startDate: null,
      endDate: null,
      discountStarted: false
    };
  }

  onSubmit() {
    if (this.productForm.valid) {
      const formData = new FormData();
      
      // Append form data
      formData.append('Name', this.productForm.get('name')?.value);
      formData.append('Description', this.productForm.get('description')?.value);
      formData.append('Price', this.productForm.get('price')?.value);
      formData.append('QuantityInStock', this.productForm.get('quantityInStock')?.value);
      formData.append('CategoryId', this.productForm.get('categoryId')?.value);
      formData.append('BrandId', this.productForm.get('brandId')?.value);
  
      // Append the main image if available
      if (this.mainImage) {
        formData.append('ImageFile', this.mainImage);
      }
  
      // Append additional images
      if (this.additionalImages.length > 0) {
        for (let i = 0; i < this.additionalImages.length; i++) {
          formData.append('ImagesFiles', this.additionalImages[i]);
        }
      }
  
      // Append discount if available
      if (this.discount) {
        formData.append('Discount', JSON.stringify(this.discount));
      }
  
      // Log for debugging purposes
      console.log('Discount:', this.discount);
      console.log('FormData:', formData);
  
      // Call the service to submit the form data
      this.productService.UpdateProduct(formData,this.product.id).subscribe({
        next: data => {
          console.log('Product added successfully:', data);
          // Handle success (e.g., reset form, navigate to another page)
        },
        error: error => {
          console.error('Error adding product:', error);
          // Handle error (e.g., show error message to the user)
        }
      });
    }
  }
  
}