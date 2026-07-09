import { defineConfig, devices } from '@playwright/test';

export default defineConfig({
  testDir: './e2e',
  fullyParallel: true,
  forbidOnly: !!process.env.CI,
  retries: process.env.CI ? 2 : 0,
  workers: process.env.CI ? 1 : undefined,
  reporter: 'list',
  use: {
    baseURL: 'http://empresa1.localhost:4200',
    trace: 'on-first-retry',
    viewport: { width: 1280, height: 720 },
  },
  projects: [
    {
      name: 'tenant-chrome',
      testMatch: /tenant-flows\.spec\.ts/,
      use: {
        ...devices['Desktop Chrome'],
        channel: 'chrome',
        baseURL: 'http://empresa1.localhost:4200',
      },
    },
    {
      name: 'admin-chrome',
      testMatch: /admin-flows\.spec\.ts/,
      use: {
        ...devices['Desktop Chrome'],
        channel: 'chrome',
        baseURL: 'http://admin.localhost:4200',
      },
    },
  ],
  webServer: {
    command: 'npx ng serve --host 0.0.0.0 --port 4200 --disable-host-check',
    url: 'http://localhost:4200',
    reuseExistingServer: !process.env.CI,
    timeout: 120_000,
  },
});
