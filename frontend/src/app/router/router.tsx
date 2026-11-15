import { createBrowserRouter } from 'react-router-dom';
import { MainLayout } from '@/widgets/layout';
import { HomePage } from '@/pages/home';

export const router = createBrowserRouter([
  {
    path: '/',
    element: <MainLayout />,
    children: [{ index: true, element: <HomePage /> }],
  },
]);
