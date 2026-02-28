import { Component, DestroyRef, OnInit, OnDestroy, inject, ChangeDetectorRef, ViewChild, ElementRef, AfterViewInit } from '@angular/core';
import { AsyncPipe } from '@angular/common';
import { takeUntilDestroyed } from '@angular/core/rxjs-interop';
import { RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { AuthService } from '../../core/services/auth.service';
import { CartService } from '../../core/services/cart.service';
import { ToastContainerComponent } from '../../shared/components/toast/toast.component';

@Component({
  selector: 'app-public-layout',
  standalone: true,
  imports: [RouterOutlet, RouterLink, RouterLinkActive, AsyncPipe, ToastContainerComponent],
  templateUrl: './public-layout.component.html',
  styleUrl: './public-layout.component.scss'
})
export class PublicLayoutComponent implements OnInit, OnDestroy, AfterViewInit {
  private readonly destroyRef = inject(DestroyRef);
  private readonly cartService = inject(CartService);
  private readonly cdr = inject(ChangeDetectorRef);
  public authService = inject(AuthService);
  readonly cartCount$ = this.cartService.cartCount$;

  @ViewChild('topObserver') topObserver!: ElementRef;
  private observer: IntersectionObserver | null = null;
  isScrolled = false;

  ngOnInit() {
  }

  ngAfterViewInit() {
    this.observer = new IntersectionObserver(([entry]) => {
      const scrolled = !entry.isIntersecting;
      if (this.isScrolled !== scrolled) {
        this.isScrolled = scrolled;
        this.cdr.detectChanges();
      }
    }, { threshold: 0 });

    if (this.topObserver?.nativeElement) {
      this.observer.observe(this.topObserver.nativeElement);
    }
  }

  ngOnDestroy() {
    if (this.observer) {
      this.observer.disconnect();
    }
  }

  constructor() {
    this.authService.isAuthenticated$
      .pipe(takeUntilDestroyed(this.destroyRef))
      .subscribe((isAuthenticated) => {
        if (!isAuthenticated) {
          this.cartService.resetCartCount();
          return;
        }

        this.cartService.refreshCartCount().subscribe();
      });
  }

  logout(): void {
    this.authService.logout();
  }
}