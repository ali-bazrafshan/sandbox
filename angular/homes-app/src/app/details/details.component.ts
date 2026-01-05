import { Component, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute } from '@angular/router';
import { HomeComponent } from '../home/home.component';
import { HousingLocation } from '../housing-location';

@Component({
  selector: 'app-details',
  standalone: true,
  imports: [CommonModule],
  template: `
    <p>
      {{housingLocation.id}}
    </p>
  `,
  styleUrls: ['./details.component.css']
})
export class DetailsComponent {
  route: ActivatedRoute = inject(ActivatedRoute);
  housingLocation!: HousingLocation;

  constructor() {
    let housingLocationId = Number(this.route.snapshot.params["id"]);
    this.housingLocation = new HomeComponent().housingLocationList.find(hl => hl.id == housingLocationId)!;
  }
}