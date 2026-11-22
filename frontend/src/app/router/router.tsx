import { createBrowserRouter } from 'react-router-dom';
import { MainLayout, SidebarLayout } from '@/widgets/layout';
import { HomePage } from '@/pages/home';
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
    ],
  },
]);