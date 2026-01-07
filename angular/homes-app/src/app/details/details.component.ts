import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { HousingLocation } from '../housing-location';
import { HousingService } from '../housing.service';
import { FormControl, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { Person } from '../person';

@Component({
  selector: 'app-details',
  standalone: true,
  imports: [CommonModule, ReactiveFormsModule],
  template: `
  <article>
    <img class="listing-photo" [src]="housingLocation?.photo">
    <section class="listing-description">
      <h2 class="listing-heading">{{housingLocation?.name}}</h2>
      <p class="listing-location">{{housingLocation?.city}}, {{housingLocation?.state}}</p>
    </section>
    <section class="listing-features">
      <h2 class="section-heading">About this housing location</h2>
      <ul>
        <li>Units available: {{housingLocation?.availableUnits}}</li>
        <li>Does this location have wifi: {{housingLocation?.wifi}}</li>
        <li>Does this location have laundry: {{housingLocation?.laundry}}</li>
      </ul>
    </section>
    <section class="listing-apply">
      <h2 class="section-heading">Apply to live here</h2>
      <form [formGroup]="applyForm" (submit)="submitApplication()">
        <label for="first-name">First Name</label>
        <input id="first-name" type="text" formControlName="firstName" [(ngModel)]="model.firstName">

        <label for="last-name">Last Name</label>
        <input id="last-name" type="text" formControlName="lastName" [(ngModel)]="model.lastName">

        <label for="email">Email</label>
        <input id="email" type="email" formControlName="email" [ngModel]="model.email">

        <button type="submit" class="primary">Apply</button>
      </form>
    </section>
  </article>
  `,
  styleUrls: ['./details.component.css']
})
export class DetailsComponent {
  housingService: HousingService = inject(HousingService);
  route: ActivatedRoute = inject(ActivatedRoute);
  housingLocation: HousingLocation | undefined;
  applyForm = new FormGroup({
    firstName: new FormControl(''),
    lastName: new FormControl(''),
    email: new FormControl('')
  });
  model: Person = new Person();

  constructor() {
    const housingLocationId = Number(this.route.snapshot.params["id"]);
    try
    {
      this.housingLocation = this.housingService.getHousingLocationById(housingLocationId);
    }
    catch
    {
    }
  }

  submitApplication() {
    //  this.housingService.submitApplication(
    //   this.applyForm.value.firstName ?? '',
    //   this.applyForm.value.lastName ?? '',
    //   this.applyForm.value.email ?? '',
    // );

    this.housingService.submitApplication(
      this.model.firstName,
      this.model.lastName,
      this.model.email
    );
  }
}