import { BreakpointObserver, Breakpoints } from '@angular/cdk/layout';
import { Component, inject, ViewChild, ChangeDetectionStrategy } from '@angular/core';
import { Router, RouterLink, RouterLinkActive, RouterOutlet } from '@angular/router';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';
import { MatListModule } from '@angular/material/list';
import { MatMenuModule } from '@angular/material/menu';
import { MatSidenav, MatSidenavModule } from '@angular/material/sidenav';
import { MatToolbarModule } from '@angular/material/toolbar';
import { AsyncPipe } from '@angular/common';
import { map, shareReplay } from 'rxjs';
import { AuthService } from '../../core/auth/auth.service';
import { TenantService } from '../../core/tenant/tenant.service';

interface NavItem {
  label: string;
  route: string;
  icon: string;
}

@Component({
  selector: 'app-shell',
  standalone: true,
  imports: [
    RouterOutlet,
    RouterLink,
    RouterLinkActive,
    MatSidenavModule,
    MatToolbarModule,
    MatIconModule,
    MatButtonModule,
    MatListModule,
    MatMenuModule,
    AsyncPipe,
  ],
  templateUrl: './shell.component.html',
  changeDetection: ChangeDetectionStrategy.Default,
  styleUrl: './shell.component.scss',
})
export class ShellComponent {
  @ViewChild(MatSidenav) sidenav?: MatSidenav;

  private readonly breakpointObserver = inject(BreakpointObserver);
  private readonly authService = inject(AuthService);
  private readonly tenantService = inject(TenantService);
  private readonly router = inject(Router);

  readonly isHandset$ = this.breakpointObserver
    .observe([Breakpoints.Handset, Breakpoints.TabletPortrait])
    .pipe(
      map((result) => result.matches),
      shareReplay({ bufferSize: 1, refCount: true }),
    );

  get subdomain(): string {
    return this.tenantService.getSubdomain();
  }

  get userName(): string {
    return this.authService.currentUser()?.nome ?? 'Usuário';
  }

  get navItems(): NavItem[] {
    return this.tenantService.isAdminMode()
      ? [{ label: 'Empresas', route: '/empresas', icon: 'business' }]
      : [
          { label: 'Dashboard', route: '/dashboard', icon: 'dashboard' },
          { label: 'Usuários', route: '/usuarios', icon: 'people' },
        ];
  }

  toggleSidenav(): void {
    this.sidenav?.toggle();
  }

  closeSidenavOnMobile(): void {
    if (this.breakpointObserver.isMatched('(max-width: 959px)')) {
      this.sidenav?.close();
    }
  }

  logout(): void {
    this.authService.logout();
    void this.router.navigate(['/login']);
  }
}
