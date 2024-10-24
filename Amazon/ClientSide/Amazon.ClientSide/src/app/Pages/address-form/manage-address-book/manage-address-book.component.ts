import { Component,OnInit,Output,EventEmitter} from '@angular/core';
import { AddressService } from '../../../Services/address.service';
import { Address } from '../../../Models/address';
import { CommonModule } from '@angular/common';
import { response } from 'express';
import { NgModule } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { error } from 'console';
import { delay, Subscription } from 'rxjs';
import { ToastrService } from 'ngx-toastr';

@Component({
  selector: 'app-manage-address-book',
  standalone: true,
  imports: [CommonModule,FormsModule],
  templateUrl: './manage-address-book.component.html',
  styleUrl: './manage-address-book.component.css'
})
export class ManageAddressBookComponent implements OnInit
{
  
  savedAddresses: Address[] = [];
  @Output() addressAdded = new EventEmitter<void>();

  constructor(private addressService: AddressService, private router: Router, private toastr: ToastrService) {}
  address:Address = new Address('','','','','','','','','','','');
  showAddAddressForm = false;
  isEditMode: boolean = false;
  selectedAddress: Address | null = null;

  deleteSub: Subscription | null;

  ngOnInit(): void 
  {
    this.fetchSavedAddresses();
  }

  onSelectAddress(address: Address): void {
    this.selectedAddress = address;
  }
  //Fetch address saved in database and display them in the boxes

  openAddForm() {
    this.isEditMode = false;
    this.address = this.address
    this.showAddAddressForm = true;
  }

  openEditForm(selectedAddress: Address) 
  {
    this.isEditMode = true;
    this.address = { ...selectedAddress };
    this.showAddAddressForm = true;
  }
  
  fetchSavedAddresses(): void 
  {
    this.addressService.getSavedAddresses().subscribe(
      (addresses) => {
        this.savedAddresses = addresses;
      },
      (error) => {
        console.error('Error fetching addresses:', error);
      }
    );
  }

  // editAddress(selectedAddress: Address) {
  //   this.address = selectedAddress;
  //   this.showEditForm = true;
  // }

  editAddress(selectedAddress: Address){
    //if(this.selectedAddress){
      this.address = selectedAddress;
      this.addressService.updateAddress(this.address).subscribe({
        next:(response) =>{
          this.toastr.success('Address Modified');
          window.location.reload();
        },
        error:(error) => {
          console.log('Error updating address',error);
        }
      });
    //}
  }

  // editAddress(selectedAddress: Address)
  // {
      
  //   console.log('cond true')
  //   this.address = selectedAddress;
  //   this.addressService.updateAddress(this.address).subscribe({
  //   next:(response) =>{
  //     console.log(this.address);
  //     this.toastr.success('Address Modified');
  //     this.toggleAddAddressForm();
  //     this.fetchSavedAddresses();
  //   },
  //   error:(error) => {
  //     console.log('Error updating address',error);
  //   }
  // });
    
  // }

  loadAddresses() 
  {
    this.addressService.getSavedAddresses().subscribe(
      (data) => {
        this.savedAddresses = data;
        window.location.reload();
      },
      (error) => {
        console.error('Error loading addresses', error);
      }
    );
  }

  deleteAddress(addressId: string) 
  {
    this.deleteSub = this.addressService.deleteAddress(addressId).subscribe({
      next: () => {
        console.log('deleted');
        // const currentUrl = this.router.url;
        // this.savedAddresses.pop();
        // this.loadAddresses();
        this.fetchSavedAddresses();
        // this.toastr.success('Address Deleted');
      },
      error:(error) =>{
        console.error('Error deleting address', error);
        this.fetchSavedAddresses();
        // this.savedAddresses.pop();
      }
    });
  }

  //attached with cancel button to close the form
  closeForm()
  {
    this.showAddAddressForm = false;
    this.address = this.address;
    this.addressAdded.emit();
  }

  toggleAddAddressForm(): void
  {
    this.showAddAddressForm = !this.showAddAddressForm;
  }

  loadAddressToEdit(selectedAddress: Address) {
    this.address = selectedAddress;
    this.isEditMode = true;
  }

  onSubmit() {
    if (this.isEditMode) 
    {
      this.editAddress(this.address);
    } 
    else 
    {
      this.onSubmitNewAddress();
    }
  }
  //binded to the submit button that add the input of form into the database
  onSubmitNewAddress():void{
    this.addressService.addAddresses(this.address).subscribe(
    (response)=>{
      this.fetchSavedAddresses();
      this.toggleAddAddressForm();
      this.addressAdded.emit();
      this.resetAddressForm();
    },
    (error)=>{
      console.error('Error adding address:', error);
      }
    );
  }

  onSetDefaultAddress(addressId: string):void{
    if(addressId){
      this.addressService.setDefaultAddress(addressId)
        .subscribe({
          next:(response) =>{
            // toastr.success('Changed Default Address')
            this.router.navigate(['/manage-address-book']);
          },
          error:(error) =>{
            console.log('Error setting default address',error);
        }
      });
    }
  }

  resetAddressForm(): void {
    this.address = {
      id:'',
      country: '',
      firstName: '',
      lastName: '',
      phoneNumber: '',
      streetAddress: '',
      buildingName: '',
      city: '',
      district: '',
      governorate: '',
      nearestLandMark: ''
    };
  }

}
