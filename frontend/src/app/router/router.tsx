import { createBrowserRouter } from 'react-router-dom';
import { MainLayout, SidebarLayout } from '@/widgets/layout';
import { HomePage } from '@/pages/home';
import { ProfilePage } from '@/pages/profile';
import { CatalogPage } from '@/pages/catalog';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <MainLayout />,
    children: [
      {
        element: <SidebarLayout />,
        children: [
          { index: true, element: <HomePage /> },
          { path: 'catalog', element: <CatalogPage /> },
        ],
      },
      { path: 'profile', element: <ProfilePage /> },
    ],
  },
]);
