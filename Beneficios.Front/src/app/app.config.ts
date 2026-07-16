import { ApplicationConfig, LOCALE_ID, provideZoneChangeDetection } from '@angular/core';

import { provideHttpClient, withInterceptors, withXhr } from '@angular/common/http';

import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';

import { provideRouter, withRouterConfig } from '@angular/router';

import { MAT_ICON_DEFAULT_OPTIONS } from '@angular/material/icon';
import { MatPaginatorIntl } from '@angular/material/paginator';

import { routes } from './app.routes';

import { authInterceptor } from './core/auth/auth.interceptor';

import { httpErrorInterceptor } from './core/http/http-error.interceptor';
import { PtBrPaginatorIntl } from './shared/i18n/pt-br-paginator.intl';



export const appConfig: ApplicationConfig = {

  providers: [

    provideZoneChangeDetection({ eventCoalescing: true }),

    provideRouter(

      routes,

      withRouterConfig({ onSameUrlNavigation: 'reload' }),

    ),

    provideAnimationsAsync(),

    provideHttpClient(withXhr(), withInterceptors([authInterceptor, httpErrorInterceptor])),

    { provide: LOCALE_ID, useValue: 'pt-BR' },

    { provide: MAT_ICON_DEFAULT_OPTIONS, useValue: { fontSet: 'material-icons' } },

    { provide: MatPaginatorIntl, useClass: PtBrPaginatorIntl },

  ],

};


