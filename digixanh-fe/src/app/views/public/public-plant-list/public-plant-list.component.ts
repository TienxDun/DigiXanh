import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';

interface PublicPlant {
  name: string;
  description: string;
  price: string;
}

@Component({
  selector: 'app-public-plant-list',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './public-plant-list.component.html',
  styleUrl: './public-plant-list.component.scss'
})
export class PublicPlantListComponent {
  plants: PublicPlant[] = [
    {
      name: 'Cây Monstera',
      description: 'Phù hợp không gian nội thất, dễ chăm sóc.',
      price: '450.000đ'
    },
    {
      name: 'Cây Lưỡi Hổ',
      description: 'Thanh lọc không khí tốt, thích hợp văn phòng.',
      price: '220.000đ'
    },
    {
      name: 'Cây Trầu Bà',
      description: 'Tăng mảng xanh cho bàn làm việc và phòng khách.',
      price: '180.000đ'
    }
  ];
}